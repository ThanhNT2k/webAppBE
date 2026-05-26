# Stage 1: Build ứng dụng bằng SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj từ thư mục gốc vào và restore NuGet
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ mã nguồn còn lại và tiến hành Publish
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Khởi chạy môi trường Runtime nhỏ gọn
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng chạy cho Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Chạy trực tiếp file dll sinh ra (Docker sẽ tự quét file .dll duy nhất trong thư mục)
ENTRYPOINT ["sh", "-c", "dotnet $(ls *.dll | head -n 1)"]