echo off

IF [%1]==[] goto noparam

echo "Build project ..."
dotnet publish ..\src\MyLab.AsyncProcessor.Api\MyLab.AsyncProcessor.Api.csproj -c Release -o .\out\app

echo "Build image '%1' and 'latest'..."
docker build -t ghcr.io/mylab-search-fx/indexer:%1 -t ghcr.io/mylab-search-fx/indexer:latest .

echo "Publish image '%1' ..."
docker push ghcr.io/mylab-search-fx/indexer:%1

echo "Publish image 'latest' ..."
docker push ghcr.io/mylab-search-fx/indexer:latest

goto done

:noparam
echo "Please specify image version"
goto done

:done
echo "Done!"