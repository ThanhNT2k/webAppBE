# Stage 1: Build ứng dụng bằng SDK .NET 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy TOÀN BỘ mã nguồn vào trước để đảm bảo đầy đủ file .sln và .csproj
COPY . .

# Thực hiện restore và publish đồng thời dựa trên file chạy chính
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime nhỏ gọn bằng .NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng mạng cho Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Tìm và chạy file .dll của dự án
ENTRYPOINT ["sh", "-c", "dotnet $(ls *.WebApi.dll 2>/dev/null || ls *.dll | head -n 1)"]