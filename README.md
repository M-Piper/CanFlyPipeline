# CanFly Flight Logging App
The CanFly web application is designed specifically for Canadian pilots looking to log their hours. Particularly during the training phase, logging hours is important not only to fulfill legal requirements but also ensure that students can avoid overspending on costly flights. 
This instance of the logging app is currently being used as a demo product to showcase the potential for this idea to be expanded into a full enterprise level software solution.
# Introduction 
Development contributions are not being sought at this moment in time but the public is more than welcome to install and try out the demo project.
# Installation

## Frontend
To run this project locally, follow these steps, Visual Studio Code is the recommended for the frontend:
Clone the repository:
`git clone https://github.com/M-Piper/CanFly---Copy`
Install dependencies:
`cd your-project
npm install`
Start the Angular development server:
`ng serve`
Navigate to http://localhost:4200/ in your browser to view the application.

## Backend
For the backend installation, you need to first clone the CanFlyPipeline repository:
Clone the backend repository:
`git clone https://github.com/M-Piper/CanFlyPipeline`

Install backend dependencies and configure the SQL database:
Use the SQL script "canfly.sql" found in the home directory to create the database.
Configure the SQL database connection in the backend to match your local SQL database. The variable named "CanFlyDBConn" needs to be updated accordingly.

Adjust the frontend (CanFly---Copy) to communicate with the backend:
In logs.service.ts, line 18, update the API URL to point to your backend:
`private readonly APIUrl = "YOUR_API_ENDPOINT_URL_HERE/api/CanFly/";`
Replace "YOUR_API_ENDPOINT_URL_HERE" with the URL for your backend.

Adjust the backend (CanFlyPipeline) to communicate with the frontend:
In program.cs, update line 46 to use the URL for the frontend as used on your local device:
`"https://YOUR_FRONTEND_URL_HERE/swagger/v1/swagger.json";`
Replace "YOUR_FRONTEND_URL_HERE" with the URL for your frontend.

# Usage
This project is a web-based application built with Angular and TypeScript. It allows users to manage flight logs and view their aviation records. This demo version does not yet allow new users to log in and create accounts but the demo account for a student names “Sarah Demo” is hard coded into the scripts.

Landing page: This page shows key information that would be useful at a glance to student or professional pilots. Some of the information is hard coded in as a placeholder example – notably the display box for ratings/licenses being worked toward. Other fields, such as total hours, are dynamically generated from the logbook.

View Logs: The flight logs page includes details such as date, aircraft type, and flight duration. The database includes many more fields, which are currently hidden but that could be changed by altering the columnAliases list in logbook.components.ts (rows 19-54) to include other fields as identified in the SQL database.

Add New Flight: Use the "Add Flight" feature to input details of a new flight, including date, aircraft information, and flight duration.

Delete Logs: You can delete existing flight logs if needed.

# API Documentation
This project's backend utilizes a C# middleware, which interacts with a database using Swagger for API calls. The API endpoints are documented using Swagger, providing a comprehensive guide on how to interact with the backend services.
