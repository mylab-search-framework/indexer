# MyLab.Search.Indexer

[![Docker image](https://img.shields.io/static/v1?label=docker&style=flat&logo=docker&message=v1.0.1&color=blue)](https://github.com/mylab-search-fx/indexer/pkgs/container/indexer)

Индексирует данные из базы данных и/или `RabbitMQ` в `ElasticSearch`.

## Обзор

![](./doс/mylab-search-index.png)

На схеме выше показаны все участники процесса индексации и их связи.

Процесс индексации определяет следующие особенности:

* данные могут быть получены из очереди (`RabbitMQ`) и/или из БД в зависимости от настроек;
* база данных является приоритетным источником, т.е. если данные об индексируемой сущности приходят через очередь, то они перезапишутся данными из БД в ближайшей итерации по расписанию;
* при совместном использовании, очерди и БД, назначение очереди - максимально быстро доставить данные для индексирования.

## Индексация через очередь

При передаче данных через очередь, сущности передаются по одному в формате `JSON`. Не в зависимости от того, изменена ли сущетсвующая сущность или создаётся новая, передаваться в сообщении должны актуальные данные в том виде, в каком они будут индексироваться. В случае, если сущность новая, то запись о ней появится в `ElasticSearch`. Если сущность была изменена и её индесированная копия уже есть в `ElasticSearch`, то данные будут заменены. Сопоставление переданной сущности и индексированной копии происходит по полю, указанному в настройках `Indexer` как поле-идентификатор. 

Ниже приведён пример отправки тестовой сущности. В настройках `Indexer` поле-идентификатор `Id`:

```C#
public class SearchTestEntity
{
    public long Id { get; set; }
    public string Value { get; set; }
}
```

Тело сообщения #1 в очереди:

```json
{"Id":2,"Value":"foo"}
```

Результат поиска #1 индексированной сущности (лишнее обрезано):

```json
{ "_source": { "Id": 2, "Value": "foo" } }
```

Тело сообщения #2 в очереди:

```json
{"Id":2,"Value":"bar"}
```

Результат поиска #2 индексированной сущности (лишнее обрезано):

```json
{ "_source": { "Id": 2, "Value": "bar" } }
```

## Индексация через БД

Выборка даных для индексации осуществляется в соответствии с настройками и "отступом" последней выборки - `seed`. В зависимости от выбранной стратегии выборки, seed может принимать целочисленное значение или дату и время. В конце каждой итерации новый `seed` сохраняется в локальный файл. В начале очередной итерации - загружается. Если это первая итерации или файл не найден, то используется минимальное значение соответствующего типа.

Индексация через базу данных осуществляется по вызову планировщика и подразумевает следующий алгоритм:

* определяется `seed`(состояние), с которого начнётся выборка (загружается ранее сохранённый или минимальное значение);
* загрузка и индексирование данных по частям (paging) или целиком в зависимости от настроек:
  * происходит выборка данных;
  * данные оптравляются в `ElasticSearch` для индексации;
* определяется новый `seed` и сохраняется в файл.

Строка запроса в БД указывается в конфигурации и представляет из себя `SQL` запрос, в котором доступны следующие переменные:

- `seed` - "отсутп" очередного запроса;
- `offset` - сдвиг выборки 
- `limit` - предел выборки

Для индексации только новых данных необходимо применить в запросе переменную `seed` в соответствии с выбранной стратегией. Для реализации порциональной выборки и иендекцации в рамках одной итерации, необходимо в запросе испольщовать параметры `offset` и `limit`. 

Ниже приведён пример использования всех параметров в контексте БД `sqlite`:

```sql
select * from foo_table where LastModified > @seed limit @limit offset @offset
```

### Стратегия `Add`

Испольуется в случае, если данные только добавляются. Например - протокол действий. 

При этой стратегии:

* идентификатор сущности должен принимать целочисленные значения;
* очередная сущность должна иметь идентификатор с большим значением, чем предыдущая;
* `seed` будет иметь самое большое значение идентификатора сущности из последней итерации;
* нужно использовать `seed` в запросе для сравнения с полем-идентификатором.

Пример `sql` запроса:

```sql
select * from foo_table where Id > @seed
```

### Стратегия `Update`

Испольуется в случае, если данные могут добавляться и имзенять. 

При этой стратегии:

* идентификатор сущности может быть любого типа;
* должно быть поле типа дата+время (например, `LastModified`), которе будет обновляться при обновлении сущности;
* `seed` будет иметь самое большое значение даты+времени "`LastModified`" из последней итерации;
* нужно использовать `seed` в запросе для сравнения с полем "`LastModified`".

Пример `sql` запроса:

```sql
select * from foo_table where LastModified > @seed
```

## Удаление из индекса

Данное решение не реализует удаление индексированных данных. Например, при удалении сущности из БД. 

Подразумеваются следующие варианты решения:

* самостоятельно реализовать удаление идексируемых сущностей;
* не удалять сущности, а помечать их как удалённые, что с точки зрения `Indexer` является изменением сущности.

## Создание индекса

Если в процессе индексирования, `Indexer` обнаружит, что целевой индекс в `ElasticSearch` отсутствует, то он попытается его сздать. При этом, будут использованы настройки в соответствии со стратегией определения настроек инлекса из параметра конфигурации приложения `Indexer.NewIndexStrategy`, который может принимать значения: `Auto`/`File`.

### Cтратегия  `Auto`

По этой стратегии, `Indexer` в настройках указывает только сопоставление сущности (`mapping`). Сопосталвение вычисляется атоматически на основании информации о первой сущности из полученной выборки или о сущности из сообшения очереди. 

#### Сопоставление сущности из очереди

Для вычилсения сопоставления на основе сущности, полученной из очереди, применяется следующий алгоритм:

* сопоставляются все поля сущности;
* используется оригинальное имя поля;
* тип поля определяется следующим образом:
  * `boolean` - если [валидное значение](https://docs.microsoft.com/ru-ru/dotnet/api/system.boolean.tryparse?view=net-5.0) [bool](https://docs.microsoft.com/ru-ru/dotnet/api/system.boolean?view=net-5.0);
  * `long` - если [валидное значение](https://docs.microsoft.com/ru-ru/dotnet/api/system.int64.tryparse?view=net-5.0#System_Int64_TryParse_System_String_System_Globalization_NumberStyles_System_IFormatProvider_System_Int64__) [long](https://docs.microsoft.com/ru-ru/dotnet/api/system.int64?view=net-5.0) для [инвариантной культуры](https://docs.microsoft.com/ru-ru/dotnet/api/system.globalization.cultureinfo.invariantculture?view=net-5.0);
  * `double` - если [валидное значение](https://docs.microsoft.com/ru-ru/dotnet/api/system.double.tryparse?view=net-5.0) [double](https://docs.microsoft.com/ru-ru/dotnet/api/system.double?view=net-5.0)  для [инвариантной культуры](https://docs.microsoft.com/ru-ru/dotnet/api/system.globalization.cultureinfo.invariantculture?view=net-5.0);
  * `date` - если [валидное значение](https://docs.microsoft.com/ru-ru/dotnet/api/system.datetime.tryparse?view=net-5.0#System_DateTime_TryParse_System_String_System_DateTime__) [DateTime](https://docs.microsoft.com/ru-ru/dotnet/api/system.datetime?view=net-5.0);
  * `text` - в остальных случаях.

#### Сопостовление сущности из БД

Для вычилсения сопоставления на основе сущности, полученной из БД, применяется следующий алгоритм:

* сопоставляются все поля сущности;
* используется оригинальное имя поля;
* тип поля определяется следующим образом:
  * `boolean` - если имя типа поля содержит `bool` или равно `bit`;
  * `long` - если имя типа поля содержит `int`;
  * `double` - если имя типа поля равно одному из значений: `decimal`, `double`, `float`, `single`, `real`;
  * `date` - если имя типа поля содержит `date`;
  * `text` - если имя типа поля содержит `char` и в остальных случаях;

 ### Стратегия `File`

По этой стратегии запрос создания индекса загружаются из файла, путь к которому задаётся параметром конфигурации `Indexer.NewIndexRequestFile`.

Содержимое файла должно быть в формате `JSON` и соответствовать [документации от ElasticSearch](https://www.elastic.co/guide/en/elasticsearch/reference/current/indices-create-index.html).

## Конфигурирование

Настроки конфигурации делятся на следующие группы, представленные узлами конфигурации:

* `DB` - настройки работы с БД;
* `MQ` - настройки работы с `RabbitMQ`;
* `ES` - настройки работы с `ElasticSearch`;
* `Indexer` - настройки логики индексирования. 

### `DB` настроойки

Формат узла конфигурации должен соответствовать формату [MyLab.Db со строкой подключения по умолчанию](https://github.com/mylab-tools/db#%D0%B4%D0%B5%D1%82%D0%B0%D0%BB%D1%8C%D0%BD%D0%BE%D0%B5-%D0%BE%D0%BF%D1%80%D0%B5%D0%B4%D0%B5%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5). Кроме того, в узле должны быть указаны дополнительные параметры:

* `Query` - запрос выборки данных для индексации;
* `EnablePaging` - флаг включения постраничной загрузки данных:`true`/`false`. `false` - по умолчанию;
* `PageSize` - размер страницы. Обязателен если устанолен `EnablePaging`;
* `Strategy` - стратегия определения записей для индексации: `Add`/`Update`; 
* `Provider` - имя поставщика данных (характеризует субд):
  * `sqlite`
  * `mysql`
  * `oracel`

Пример узла конфигурации `DB`:

```json
{
  "DB": {
    "User": "foo",
    "Password": "bar",
    "ConnectionString": "Server=myServerAddress;Database=myDataBase;Uid={User};Pwd={Password};",
    "Provider": "sqlite",
    "Query": "select * from test_tb where Id > @seed limit @limit offset @offset",
    "Strategy": "Add",
    "EnablePaging": "true",
    "PageSize": "100"
  }
}
```

### `MQ` настройки

Формат узла конфигурации должен соответствовать формату [MyLab.Mq](https://github.com/mylab-tools/mq#%D0%BA%D0%BE%D0%BD%D1%84%D0%B8%D0%B3%D1%83%D1%80%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5). Кроме того, в узле должно быть указано имя очереди - параметр `Queue`.

```json
{
  "MQ": {
    "Host" : "myhost.com",
    "VHost" : "test-host",
    "User" : "foo",
    "Password" : "foo-pass",
    "Queue": "my-queue"
  }
}
```

### `ES` настройки

Данный узел должен содержать следущие параметры:

* `Url` - адрес подключения к `ElasticSearch` ;
* `Defaultindex` - целевой индекс.

Пример узла конфигурации `ES`:

```json
{
  "ES": {
    "Url" : "http://localhost:9200",
    "Defaultindex" : "entities"
  }
}
```

### `Indexer` настройки логики индексирования

Данный узел должен содержать следущие параметры:

* `IdProperty` - имя свойства, идентифицирующее сущность;
* `LastChangeProperty` - имя свойства, содержащее дату и время актуализации данных сущности (создания или изменения). Обязательно, если `Db.Strategy == 'Update'`;
* `NewIndexStrategy` - стратегия создания индекса: `Auto`/`File`;
*  `NewIndexRequestFile` - путь к файлу запроса создания индекса. По умолчанию `/etc/mylab-indexer/new-index-request.json`.

Пример узла конфигурации `Indexer`:

```json
{
  "Indexer": {
    "IdProperty" : "Id",
    "LastChangeProperty" : "LastChangeDt",
    "NewIndexStrategy": "Auto"
  }
}
```

## Развёртывание

Развёртывание сервиса предусмотрено в виде `docker`-контейнера.

Пример `docker-compose.yml` файла:

```yaml
version: '3.2'

services:
  mylab-search-indexer:
    container_name: mylab-search-indexer
    image: ghcr.io/mylab-search-fx/indexer:latest
    volumes:
    - ./appsettings.json:/app/appsettings.json
    - ./new-index-request.json:/etc/mylab-indexer/new-index-request.json
```

## Рекомендации

### 1. Используйте очередь только при необходиомости

Используйте очередь совместно с БД только если требуется оперативная индексация. Например, для сущностей, которые создают и редактируют пользователи через интерфейс после этого переходят в список, который запрашивается в поисковике. 

Использовании очереди с БД добавляет связанности и сложности решению.

### 2. Не используйте звёздочку `*` в SQL запросах

Не используйте хвёхдочку в запросах `sql` для выборки данных для индексируемых сущностей. Использование звёздочки `*` для указания выборки всех полей приведёт к бесконтрольному изменению состава индексируемых сущностей в случае изменения состава полей в БД. Это может привести к значительному увеличению размера индекса, замедлению индексации и поиска, а также к индексации секретной информации.

### 3. Обеспечте соответствие MQ и БД сущностей

При разработке обратите пристальноевнимание на состав сущностей, выбираемых из БД и сущностей, передаваемых через очередь. Набор полей должен точно соответствовать по перечню полей, их именам (с учётом регистра) и типам. В противном случае могут быть проблемы с поиском потерей данных.

## Диагностика

### Не найдено поле последней модификации

Симптом:

```yaml
fail: MyLab.Search.Indexer.Services.IndexerTaskLogic[0]
      Message: Last change property not found
      Time: 2021-08-06T16:28:08.069
      Labels:
        log_level: error
      Facts:
        Expected field name: LastChangeDt
        Actual fields: Id, GivenName, LastName
```

Прична:

Запрос не выбирает поле последнего изменения записи, поэтому индексатор не может его найти, чтобы вычислить максимальное значение и записать в `seed`.

Пример такого запроса:

```sql
select Id, GivenName, LastName from user where LastChangeDt > @seed
```

Решение:

Добавить поле последнего именения сущности в список получаемых полей:

```sql
select Id, GivenName, LastName, LastChangeDt from user where LastChangeDt > @seed
```

