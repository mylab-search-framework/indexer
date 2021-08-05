echo off

IF [%1]==[] goto noparam

echo "Build image '%1' and 'latest'..."
docker build --progress plain -f ./Dockerfile -t ghcr.io/mylab-search-fx/indexer:%1 -t ghcr.io/mylab-search-fx/indexer:latest ../src

goto done

:noparam
echo "Please specify image version"
goto done

:done
echo "Done!"