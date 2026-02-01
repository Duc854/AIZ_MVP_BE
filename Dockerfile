# Giai đoạn 1: Build source code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy file csproj của tất cả các project vào để restore
COPY ["AIZ_MVP_Presentation/AIZ_MVP_Presentation.csproj", "AIZ_MVP_Presentation/"]
COPY ["AIZ_MVP_Bussiness/AIZ_MVP_Bussiness.csproj", "AIZ_MVP_Bussiness/"]
COPY ["AIZ_MVP_Data/AIZ_MVP_Data.csproj", "AIZ_MVP_Data/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# 2. Restore các thư viện (nuget restore) cho project chính
RUN dotnet restore "AIZ_MVP_Presentation/AIZ_MVP_Presentation.csproj"

# 3. Copy toàn bộ source code còn lại vào
COPY . .

# 4. Build và Publish project Presentation (đầu não)
WORKDIR "/src/AIZ_MVP_Presentation"
RUN dotnet publish "AIZ_MVP_Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Tạo môi trường chạy (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Mở cổng 8080 (Chuẩn mới của .NET 8)
EXPOSE 8080

# Chạy file DLL (Tên lấy theo Project Presentation)
ENTRYPOINT ["dotnet", "AIZ_MVP_Presentation.dll"]