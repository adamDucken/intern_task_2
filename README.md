# intern task 2

REST API risinājums māju, dzīvokļu un iedzīvotāju pārvaldībai, izmantojot ASP.NET Core un Entity Framework Core.

## funkcionalitāte

* **māju pārvaldība**: CRUD operācijas māju datu uzturēšanai.
* **dzīvokļu pārvaldība**: Iespēja pievienot un rediģēt dzīvokļus, saistot tos ar konkrētām mājām.
* **iedzīvotāju uzskaite**: Iedzīvotāju datu pārvaldība ar piesaisti dzīvokļiem.
* **datu validācija**: Iebūvēta pārbaude e-pasta formātam, obligātajiem laukiem un skaitliskajām vērtībām.
* **integrācijas testi**: Visu kontrolieru gala punktu (endpoints) automātiska testēšana.
* **Swagger UI**: Interaktīva dokumentācija API testēšanai pārlūkprogrammā.

## kā palaist

### palaist API lietojumprogrammu:

```bash
dotnet run --project src/intern_task_2

```

### palaist integrācijas testus:

```bash
dotnet test

```

### būvēt visu solution:

```bash
dotnet build

```

## prasības

* .NET 8.0 vai jaunāks
* `Microsoft.EntityFrameworkCore.InMemory` (izmantots izstrādes un testēšanas nolūkos)

## projekta struktūra

* `src/intern_task_2`: API loģika, modeļi un datu piekļuves slānis.
* `tests/intern_task_2.Tests`: Integrācijas testi, kas pārbauda API darbību.
