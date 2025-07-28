# 🏢 מערכת ניהול מעליות - Elevator Management System

מערכת מתקדמת לניהול וסימולציה של מעליות בבניינים עם ממשק משתמש אינטראקטיבי וזמן אמת.

## 📁 מבנה הפרויקט

```
AdviceItamarProject/
├── backend/                    # פרויקט .NET Core Web API
│   ├── Controllers/           # בקרי API
│   │   ├── BuildingsController.cs
│   │   ├── ElevatorCallsController.cs
│   │   ├── ElevatorsController.cs
│   │   └── UsersController.cs
│   ├── Models/               # מודלי נתונים
│   │   ├── Building.cs
│   │   ├── Elevator.cs
│   │   ├── ElevatorCall.cs
│   │   ├── ElevatorCallAssignment.cs
│   │   ├── User.cs
│   │   └── ElevatorDbContext.cs
│   ├── Services/             # שירותי רקע ולוגיקה עסקית
│   │   └── ElevatorMovementService.cs
│   ├── Hubs/                # SignalR Hubs לתקשורת זמן אמת
│   │   └── ElevatorHub.cs
│   ├── Enums/               # הגדרות enum
│   │   └── ElevatorEnums.cs
│   ├── DTOs/                # Data Transfer Objects
│   │   ├── BuildingDTO.cs
│   │   ├── ElevatorCallDTO.cs
│   │   └── InsideElevatorCallDTO.cs
│   └── Program.cs           # נקודת כניסה לאפליקציה
├── front/                   # פרויקט React (יש ליצור)
│   ├── src/
│   │   ├── components/      # רכיבי React
│   │   │   ├── Navbar.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   ├── Pages/          # דפי האפליקציה
│   │   │   ├── LoginPage.tsx
│   │   │   ├── RegisterPage.tsx
│   │   │   ├── BuildingsPage.tsx
│   │   │   ├── BuildingViewPage.tsx
│   │   │   └── BuildingView.tsx
│   │   ├── App.tsx         # רכיב אפליקציה ראשי
│   │   ├── index.tsx       # נקודת כניסה
│   │   └── Buildings.css   # עיצוב מעליות
│   ├── package.json        # תלויות Node.js
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

2. **עדכן connection string** אם נדרש (ברירת מחדל עובדת עם LocalDB):
   ```csharp
   // בקובץ ElevatorDbContext.cs - הגדרת ברירת המחדל:
   "Server=localhost;Database=elevatorDB;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

3. **יצירת מסד הנתונים והרצת migrations**:
   ```bash
   cd AdviceItamarProject
   dotnet tool install --global dotnet-ef  # התקנת EF tools (פעם אחת)
   dotnet ef migrations add InitialCreate  # יצירת migration ראשוני
   dotnet ef database update              # יצירת מסד הנתונים
   ```

### הרצת השרת (Backend)

```bash
cd AdviceItamarProject
dotnet restore    # התקנת תלויות
dotnet run       # הרצת השרת
```

השרת יעלה על: `https://localhost:5285`

אמת שהשרת עובד על ידי ביקור ב: `https://localhost:5285/swagger`

### הגדרת והרצת הקליינט (Frontend)

⚠️ **שים לב**: תחילה יש ליצור את פרויקט React, ואז להעתיק את הקבצים הקיימים.

1. **יצירת פרויקט React בפעם הראשונה**:
   ```bash
   cd AdviceItamarProject
   npx create-react-app front --template typescript
   cd front
   npm install axios @microsoft/signalr react-router-dom
   npm install --save-dev @types/react-router-dom
   ```

2. **העתקת קבצי הפרויקט**:
   ```bash
   # העתק את הקבצים הבאים לתיקיית src/ החדשה:
   # - App.tsx
   # - Pages/ (כל הקבצים)
   # - components/ (כל הקבצים)  
   # - Buildings.css
   # - index.tsx
   ```

3. **הרצת הקליינט**:
   ```bash
   cd front
   npm start
   ```

הקליינט יעלה על: `http://localhost:3000`

### סדר הפעלה הנכון

1. **הפעל תחילה את השרת** (.NET):
   ```bash
   cd AdviceItamarProject
   dotnet run
   ```

2. **אחר כך הפעל את הקליינט** (React):
   ```bash
   cd front
   npm start
   ```

## 🎮 איך להשתמש במערכת

### זרימת עבודה בסיסית:
1. **הרשמה/התחברות** - צור משתמש חדש או התחבר
2. **יצירת בניין** - הגדר בניין חדש עם מספר קומות
3. **כניסה לסימולציה** - לחץ על "עבור לסימולציה"
4. **הזמנת מעלית** - לחץ על כפתורי "הזמן מעלית" בקומות השונות
5. **בחירת יעד** - כאשר המעלית מגיעה, בחר את קומת היעד
6. **צפייה בתנועה** - המעלית תנוע בזמן אמת לקומה הנבחרת

### תכונות המערכת:
- ✅ **תנועה בזמן אמת** - המעלית זז כל 2 שניות
- ✅ **טיימר אוטומטי** - דלתות נסגרות אחרי 10 שניות
- ✅ **ניהול תור** - מספר קריאות בו זמנית
- ✅ **ממשק בעברית** - כל הטקסטים בעברית
- ✅ **אנימציות חלקות** - תנועת מעלית ויזואלית

## 🛠️ טכנולוגיות שנבחרו

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

### מנגנון דלתות אוטומטי
```csharp
// דלתות נסגרות אוטומטית אחרי 10 שניות
if (doorsOpenDuration.TotalSeconds >= 10)
{
    elevator.Status = ElevatorStatus.ClosingDoors;
    Console.WriteLine($"⏰ Elevator {elevator.Id} doors closing automatically");
}
```

## 🔧 פתרון בעיות נפוצות

### שגיאות בסיס נתונים:
```bash
# אם יש בעיות עם migrations:
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### שגיאות Frontend:
```bash
# אם אין package.json:
cd AdviceItamarProject
npx create-react-app front --template typescript
cd front
npm install axios @microsoft/signalr react-router-dom
```

### בעיות CORS:
- ודא שהשרת רץ על `https://localhost:5285`
- ודא שהקליינט רץ על `http://localhost:3000`
- ה-CORS מוגדר כבר בקוד לכתובות אלו

## 📝 הערות פיתוח

- **Background Service** רץ כל 2 שניות ומעדכן מיקומי מעליות
- **SignalR** שולח עדכונים בזמן אמת לקליינט
- **Database Schema** תומך במספר בניינים ומעליות
- **מערכת תור** מטפלת במספר קריאות במקביל
- **טיימר דלתות** מונע חסימת מעליות

---

**פותח על ידי:** Itamar Pinhas  
**תאריך:** 28.07.2025  
**גרסה:** 1.0.0
