# 🏢 מערכת ניהול מעליות - Elevator Management System

מערכת מתקדמת לניהול וסימולציה של מעליות בבניינים עם ממשק משתמש אינטראקטיבי וזמן אמת.

## 📁 מבנה הפרויקט

```
elevator-system/
├── backend/                 # פרויקט .NET Core Web API
│   ├── Controllers/         # בקרי API
│   ├── Models/             # מודלי נתונים
│   ├── Services/           # שירותי רקע ולוגיקה עסקית
│   ├── Hubs/              # SignalR Hubs לתקשורת זמן אמת
│   └── Data/              # הקשר למסד הנתונים
├── frontend/               # פרויקט React
│   ├── src/
│   │   ├── components/    # רכיבי React
│   │   ├── pages/         # דפי האפליקציה
│   │   └── styles/        # קבצי עיצוב
│   └── public/
└── README.md
```

## 🚀 התקנה והרצה

### דרישות מקדימות

- **.NET 6.0 SDK** או גרסה חדשה יותר
- **Node.js 16+** ו-**npm**
- **SQL Server** או **SQL Server Express**
- **Visual Studio 2022** או **VS Code** (מומלץ)

### הגדרת מסד הנתונים

1. **התקן SQL Server Express** (אם לא מותקן):
   ```bash
   # הורד מ: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   ```

2. **עדכן connection string** ב-`appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ElevatorSystemDB;Trusted_Connection=true;"
     }
   }
   ```

3. **הרץ migrations** ליצירת מסד הנתונים:
   ```bash
   cd backend
   dotnet ef database update
   ```
   השתמשתי בMigrations  כדי  לבנות את המבנה המורכב של הפרויקט -
### הרצת השרת (Backend)

```bash
cd backend
dotnet restore
dotnet run
```

השרת יעלה על: `https://localhost:5285`

### הרצת הקליינט (Frontend)

```bash
cd frontend
npm install
npm start
```

הקליינט יעלה על: `http://localhost:3000`

##  טכנולוגיות שנבחרו

### Backend (.NET Core)

| טכנולוגיה | סיבת הבחירה |
|------------|-------------|
| **ASP.NET Core Web API** | ביצועים גבוהים, תמיכה מובנית ב-CORS, אקוסיסטם עשיר |
| **Entity Framework Core** | ORM מתקדם, Code-First approach, תמיכה במגוון מסדי נתונים |
| **SignalR** | תקשורת דו-כיוונית בזמן אמת, אידיאלי לעדכוני מעליות |
| **Background Services** | הרצת לוגיקת תנועת מעליות ברקע באופן רציף |
| **SQL Server** | מסד נתונים יציב ומהיר, תמיכה מצוינת ב-.NET |

### Frontend (React)

| טכנולוגיה | סיבת הבחירה |
|------------|-------------|
| **React 18** | ממשק משתמש דינמי, Virtual DOM, אקוסיסטם גדול |
| **TypeScript** | בטיחות טיפוסים, פיתוח מהיר יותר, תיעוד עצמי |
| **SignalR Client** | התחברות חלקה לשרת SignalR, עדכונים בזמן אמת |
| **Axios** | HTTP client פשוט ואמין, תמיכה ב-interceptors |
| **React Router** | ניווט בין דפים, routing מתקדם |

## ⚡ אלגוריתמי המעליות

### אלגוריתם תזמון FCFS (First Come First Served)

```csharp
// פונקציה לקביעת סדר עדיפויות לקריאות
private async Task<ElevatorCall> GetNextCall(Elevator elevator)
{
    return await db.ElevatorCalls
        .Where(c => c.BuildingId == elevator.BuildingId && !c.IsHandled)
        .OrderBy(c => c.CallTime)  // ראשון נכנס, ראשון יוצא
        .FirstOrDefaultAsync();
}
```

**יתרונות:**
- ✅ פשטות ביישום
- ✅ הוגנות - כל קריאה תטופל בסדר
- ✅ צפויות - זמן המתנה ניתן לחישוב

**חסרונות:**
- ❌ לא אופטימלי מבחינת זמן נסיעה
- ❌ יכול לגרום לנסיעות ארוכות מיותר

### אלגוריתם אופטימיזציה נוסף (SCAN/Elevator Algorithm)

```csharp
// אלגוריתם מתקדם יותר - נסיעה בכיוון אחד עד הסוף
private async Task<List<ElevatorCall>> OptimizeCallOrder(Elevator elevator)
{
    var calls = await GetPendingCalls(elevator.BuildingId);
    
    if (elevator.Direction == ElevatorDirection.Up)
    {
        // קודם כל הקריאות למעלה, אחר כך למטה
        return calls.OrderBy(c => c.RequestedFloor >= elevator.CurrentFloor ? 
                           c.RequestedFloor : int.MaxValue)
                   .ThenBy(c => c.RequestedFloor)
                   .ToList();
    }
    // לוגיקה דומה לכיוון למטה...
}
```


---
**פותח על ידי:** itamar pinhas
**תאריך:** 07.28.25 
