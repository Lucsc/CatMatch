# CatMatch

CatMatch est une application web permettant de voter pour savoir quel chat est le plus mignon,
Cette application est réalisée avec un front en React/Vite et un backend en C# .NET.

Une page de monitoring est intégrée dans l'application afin de voir les votes en temps réel.

---

## Pages du site

1. **Accueil / Classement des chats**  
   Affiche la liste des chats sous forme de classement, la liste est modifiable avec des filtres pour choisir une durée.

2. **Vote / Vote entre deux chats**    
   Page permettant de voter entre deux chats choisis aléatoirement

3. **Monitoring**  
   Page réservée pour le suivi en temps réel des votes grâce à SignalR.

---

## Stack technique

- **Frontend** : React 18 + Vite  
- **Backend** : C# .NET 8, ASP.NET Core  
- **Base de données** : SQL Server sur Azure (SQLite en dev local possible => si SQLite faire attention aux migrations)  
- **Temps réel** : Microsoft SignalR  
- **Déploiement** :  
  - Frontend : Vercel  
  - Backend + DB : Azure App Service + Azure SQL  

---

## Setup local

### Prérequis

- .NET 8 SDK
- Node.js 20+ et npm/yarn
- Base SQLite ou SQL Server locale

### Frontend

```bash
cd frontend
npm install
npm run dev
```
L’application front tourne par défaut sur : http://localhost:5173
Les requêtes API locales sont redirigées via le proxy Vite vers http://localhost:5213/api.


### Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet ef database update
dotnet run --project ./Backend
```

L’API tourne par défaut sur http://localhost:5213
SignalR hub pour le monitoring : /hubs/monitor


Ce projet a été entièrement réalisé par Luc Schmitt.
