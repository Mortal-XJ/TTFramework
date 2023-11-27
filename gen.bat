set WORKSPACE=.

set LUBAN_DLL=%WORKSPACE%\Excel\Luban\Luban.dll
set CONF_ROOT=%WORKSPACE%\Excel\Configs

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-bin ^
    -d bin  ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=Assets/Scripts/Code/Frame/Excel/Gen ^
    -x outputDataDir=.\Assets\HotAssets\Excel\GenerateDatas\bytes ^
    -x pathValidator.rootDir=%WORKSPACE%\Projects\Csharp_Unity_bin ^
    -x l10n.textProviderFile=*@%WORKSPACE%\luban\DesignerConfigs\Datas\l10n\texts.json

pause