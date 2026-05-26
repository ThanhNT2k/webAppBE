# Stage 1: Build ứng dụng bằng SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file project và restore các gói NuGet
COPY ["ComicBackend.WebApi/ComicBackend.WebApi.csproj", "ComicBackend.WebApi/"]
RUN dotnet restore "ComicBackend.WebApi/ComicBackend.WebApi.csproj"

# Copy toàn bộ mã nguồn và build
COPY . .
WORKDIR "/src/ComicBackend.WebApi"
RUN dotnet build "ComicBackend.WebApi.csproj" -c Release -o /app/build

# Stage 2: Publish ứng dụng
FROM build AS publish
RUN dotnet publish "ComicBackend.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Khởi chạy môi trường Runtime nhỏ gọn
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render tự động cấp phát cổng, ASP.NET Core cần nghe qua cổng PORT của môi trường
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ComicBackend.WebApi.dll"]