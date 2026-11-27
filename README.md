Чтобы применить SQL скрипт нужно:
1. Открыть pgAdmin4
2. Создать базу данных с названием "LanerDB"
3. Нажать ПКМ по созданной базе и выбрать "Restore"
4. В открывшемся окне выбрать файл в поле "Filename"
5. Далее, перейти на вкладку "Query Options" и включить флажок "Clean before restore"
6. После этого, зайти в приложение "LanerAvaApp"
7. Открыть файл "App.axaml.cs"
8. Найти строку с подключением базы данных:
```
services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=1234;Database=LanerDB;Username=username;Password=password"), ServiceLifetime.Scoped);
```
9. Подставить соответствующие значения у параметров: Port, Database, Username, Password
