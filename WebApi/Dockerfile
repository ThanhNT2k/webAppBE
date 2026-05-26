# Stage 1: Build ứng dụng bằng SDK .NET 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy toàn bộ mã nguồn từ GitHub vào Docker
COPY . .

# Tự động tìm kiếm file .csproj bất kể nó nằm ở thư mục con nào và tiến hành Publish
RUN dotnet publish */*.csproj -c Release -o /app/publish /p:UseAppHost=false || \
    dotnet publish *.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Môi trường Runtime nhỏ gọn
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng chạy cho Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Khởi chạy file .dll chính của Web API
ENTRYPOINT ["sh", "-c", "dotnet $(ls *.[Ww]eb[Aa]pi.dll 2>/dev/null || ls *.dll | head -n 1)"]