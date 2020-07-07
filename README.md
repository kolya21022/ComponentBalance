# ComponentBalanceSqlv2

Рабочее название: *ComponentBalance*  
Десктоп-приложение для учета баланса деталей в цехах предприятия.

### Требуемые базы данных:  
- Внутренняя база данных этого приложения **[ComponentBalanceDb]** (версию для разработки можно инициализировать скриптом)  
- Внешнии база данных предприятия (необходимы для расчета нового месяца).

### Параметры инициализации в IDE  
Параметры инициализации проекта на каждой машине разработчика (все файлы настроек описаны в .gitignore, дабы не перезатирать при каждом коммите): 

Параметр | Тип | Значение | Комментарий 
-------- | --- | -------- | -----------
FontSize               		| double | 15                   		| Размер шрифта
IsRunInFullscreen               | bool   | True                 		| Признак запуска приложения в полноэкранном режиме 
FoxProDbFolder_Composition      | string | w:\ComponentBalanceSqlv2\    	| Путь до таблиц с разрешенными заменами прошлых лет
FoxProDbFolder_Fox60_Arm_Limit  | string | v:\FOX60\arm\LIMIT\			| Путь до dbf таблиц лимиток предприятия 
FoxProDbFolder_Temp             | string | w:\ComponentBalanceSqlv2\Temp\   	| Путь до временных dbf таблиц приложения
FoxProDbFolder_Temp_Work        | string | w:\ComponentBalanceSqlv2\Temp\Work\  | Путь до временных вычислений в dbf таблицах приложения
FoxProDbFolder_Fox60_Arm_Base   | string | v:\FOX60\arm\BASE\    		| Путь до базовых dbf таблиц предприятия 
FoxproDbFolder_Skl              | string | v:\FOXPRO\SKL\       		| Путь до dbf таблиц складов предприятия 
FoxProDbFolder_FoxPro_Skl58     | string | v:\FOXPRO\SKL58\     		| Путь до dbf таблиц склада №58 предприятия


### Скриншоты окна настроек и рабочего приложения  

- Скриншот окна параметров IDE
![Alt text](_img/ide-config.PNG "Скриншот окна параметров IDE")  
- Скриншот окна настроек
![Alt text](_img/config.PNG "Скриншот окна настроек")  
- Скриншот окна выпущенных изделий
![Alt text](_img/app-screenshot-1.PNG "Скриншот приложения 1")  
- Скриншот окна добавления выпуска изделия
![Alt text](_img/app-screenshot-2.PNG "Скриншот приложения 2") 
- Скриншот окна корректировки баланса деталей цеха
![Alt text](_img/app-screenshot-3.PNG "Скриншот приложения 3")   
- Скриншот окна информации движения конкретной детали в цеха
![Alt text](_img/app-screenshot-4.PNG "Скриншот приложения 4")  
- Скриншот окна добавления разрешенных замен для деталей
![Alt text](_img/app-screenshot-5.PNG "Скриншот приложения 5")  
- Скриншот отчета баланса деталей цеха
![Alt text](_img/app-screenshot-6.PNG "Скриншот приложения 6")  
- Скриншот подробного отчета выпущенных изделий цеха
![Alt text](_img/app-screenshot-7.PNG "Скриншот приложения 7")  
- Скриншот доп. функционала цеха №75 (отдела АСУП)
![Alt text](_img/app-screenshot-8.PNG "Скриншот приложения 8")  
