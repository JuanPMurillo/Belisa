@echo off
rem ============================================================
rem  Belisa - Preparar la carpeta de respaldos
rem  Crea C:\RespaldosBelisa y le da permiso a SQL Server
rem  para guardar los respaldos ahi.
rem  Se ejecuta UNA sola vez: clic derecho - Ejecutar como administrador.
rem ============================================================

fsutil dirty query %systemdrive% >nul 2>&1
if errorlevel 1 (
    echo Este archivo necesita permisos de administrador.
    echo Cierra esta ventana, haz clic derecho sobre el archivo
    echo y elige "Ejecutar como administrador".
    echo.
    pause
    exit /b 1
)

echo Preparar la carpeta de respaldos de Belisa
echo.
echo Escribe el nombre de la instancia de SQL Server.
echo Si instalaste SQL Server Express con la opcion Basic, es SQLEXPRESS.
echo.
set INSTANCIA=
set /p INSTANCIA=Instancia [SQLEXPRESS]: 
if "%INSTANCIA%"=="" set INSTANCIA=SQLEXPRESS

if /I "%INSTANCIA%"=="MSSQLSERVER" (
    set CUENTA=NT SERVICE\MSSQLSERVER
) else (
    set CUENTA=NT SERVICE\MSSQL$%INSTANCIA%
)

if not exist "C:\RespaldosBelisa" mkdir "C:\RespaldosBelisa"
if not exist "C:\RespaldosBelisa" (
    echo.
    echo No se pudo crear la carpeta C:\RespaldosBelisa
    pause
    exit /b 1
)

icacls "C:\RespaldosBelisa" /grant "%CUENTA%:(OI)(CI)M"
if errorlevel 1 (
    echo.
    echo No se pudo dar el permiso a: %CUENTA%
    echo Revisa que el nombre de la instancia sea correcto.
    pause
    exit /b 1
)

echo.
echo Listo. SQL Server ya puede guardar respaldos en C:\RespaldosBelisa
echo.
pause
