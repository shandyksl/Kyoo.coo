FROM harbor.devs2023.net/base/dotnet-aspent.8.0

# 将编译到dist目录的内容拷贝进去
COPY dist/ /app/

# 工作目录
WORKDIR /app

CMD [ "dotnet", "Yoo.Core.Api.dll"]
