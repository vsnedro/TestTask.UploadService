# Тестовое задание на должность C# backend разработчика 

## Описание задачи

API принимает JSON, сохраняет данные из полей в отдельный storage и возвращает ссылки на загруженные данные. 

Задача переписать API, оптимизировав pipeline выполнения запроса таким образом, чтобы API обслуживало максимальное количество клиентов c минимальными задержками, затратами CPU и памяти.

P.S. Обратите внимание, нужно не сломать клиента.

### Пример клиента

```
using Flurl.Http;

var file = File.ReadAllBytes(@"Files\template.pptx");
var json = File.ReadAllBytes(@"Files\data.json");

await "http://localhost:6001/api/test/upload"
           .PostJsonAsync(new
           {
               Name = Guid.NewGuid(),
               File = file,
               Json = json,
           });
```


## Результат

В качестве результата мы ожидаем ссылку на git репозиторий и ссылку на собранный docker образ на DockerHub'e. 
