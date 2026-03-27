# 🏥 MediCare Connect – Online Healthcare Appointment System

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-6.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB-6.0-47A248?style=flat&logo=mongodb&logoColor=white)](https://www.mongodb.com/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> A full-stack healthcare appointment management system enabling patients to book appointments, doctors to manage consultations, and administrators to oversee operations.

---

## 👨‍💻 Developer
**Saritha Themiyadasa**  
Diploma in Software Engineering   

---

## 📌 Overview
**MediCare Connect** is a modern web-based healthcare system designed to streamline appointment booking and medical record management.

It eliminates traditional inefficiencies such as long queues, manual scheduling, and fragmented data handling by providing a centralized digital solution.

---

## ✨ Key Features

### 👤 Patient Portal
- User registration & authentication  
- Doctor search by specialty and availability  
- Real-time appointment booking  
- Online payment system  
- Medical records access  
- Appointment management (view, cancel, reschedule)  
- Doctor ratings & feedback  

---

### 👨‍⚕️ Doctor Dashboard
- View and manage appointments  
- Set availability schedule  
- Add consultation notes  
- Manage patient records  
- Update profile  

---

### 👨‍💼 Admin Panel
- Manage doctors and specialties  
- View patients and appointments  
- Analytics dashboard  
- Financial and system reports  

---

## 🛠️ Technology Stack

### Backend
- ASP.NET Core 6.0 MVC  
- C#  
- Repository Pattern  

### Database
- MongoDB  
- MongoDB Atlas  

### Frontend
- Razor Views  
- Bootstrap 5  
- JavaScript  

### Tools & Services
- Visual Studio 2022  
- Git & GitHub  
- MongoDB Compass  
- Docker  

---

## 🏗️ System Architecture


Presentation Layer (Razor Views)
↓
Controller Layer
↓
Business Logic Layer (Services & Repositories)
↓
Data Access Layer (MongoDB Driver)
↓
MongoDB Database


---

## 🚀 Getting Started

### Prerequisites
- .NET 6 SDK  
- Visual Studio 2022  
- MongoDB (Local or Atlas)  
- Git  

---

### Installation

```bash
git clone https://github.com/SarithaThemiyadasa/medicare-connect.git
cd medicare-connect
dotnet restore
```
Configuration

Update appsettings.json:

```bash
{
  "ConnectionStrings": {
    "MongoDbConnection": "mongodb://localhost:27017",
    "DatabaseName": "MediCareConnectDB"
  }
}
```
Run Application
```bash
dotnet build
dotnet run
```
📖 Usage
Test Credentials

Admin
```bash
admin@medicare.lk
admin123
```
Doctor
```bash
nimal.perera@medicare.lk
doctor123
```
Patient
```bash
kasun.r@email.com
patient123
```

📁 Project Structure
HealthcareAppointmentSystem/

├── Controllers/

├── Models/

├── Services/

├── Views/

├── wwwroot/

├── Database/

├── appsettings.json

├── Program.cs

└── README.md

🌐 Deployment
Platforms
Railway
Microsoft Azure
Steps
Connect GitHub repository
Configure environment variables
Deploy
Environment Variables
ConnectionStrings__MongoDbConnection=your_connection
ConnectionStrings__DatabaseName=MediCareConnectDB
ASPNETCORE_ENVIRONMENT=Production
🚀 Future Enhancements
Email & SMS notifications
Video consultation integration
Mobile application (iOS & Android)
Multi-language support
AI-based symptom checker
📎 Project Links
GitHub Repository: https://github.com/Saritha-115/HealthcareAppointmentSystem
Live Demo: https://medicare-connect.up.railway.app
📞 Contact

Saritha Themiyadasa

GitHub: https://github.com/Saritha-115
LinkedIn: https://linkedin.com/in/saritha-themiyadasa
📄 License

This project is licensed under the MIT License.

<div align="center">

⭐ Star this repository if you find it helpful!

Made with ❤️ by Saritha Themiyadasa

</div> 
