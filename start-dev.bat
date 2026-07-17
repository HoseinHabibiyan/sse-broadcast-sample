@echo off

echo Starting frontend server...

cd wwwroot

start cmd /k npx http-server -p 8080

timeout /t 3

start http://127.0.0.1:8080/index.html

timeout /t 2

start http://localhost:5171/scalar

cd ..

dotnet run