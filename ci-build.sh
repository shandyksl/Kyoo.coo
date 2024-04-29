
export MSBUILDDISABLENODEREUSE=1 
#dotnet clean 
dotnet restore
rm -rf dist
dotnet publish -r linux-musl-x64 --configuration Release -o dist