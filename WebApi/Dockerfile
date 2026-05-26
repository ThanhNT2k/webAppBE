# Stage 1: Build ứng dụng bằng SDK .NET 10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy toàn bộ mã nguồn từ GitHub vào Docker
COPY . .

# Tiến hành Publish trực tiếp bằng file dự án WebApi
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Môi trường Runtime nhỏ gọn của .NET 10.0
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng chạy cho Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Khởi chạy file .dll chính của dự án WebApi
ENTRYPOINT ["dotnet", "WebApi.dll"]