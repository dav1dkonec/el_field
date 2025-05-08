# ElectricFieldVis

**Semester Project for KIV/APG 2024/2025**  
The application visualizes an electrostatic field with the ability to add charges, probes, and monitor field strength over time. The project includes multiple scenarios, including dynamic charge simulations where the charge magnitude changes over time.

---

## 📁 Project Structure

```
├── src/           # Source code (.cs, .csproj, .sln)
├── bin/           # Build outputs (.exe, .dll, etc.)
├── doc/           # Documentation (PDF)
├── Build.cmd      # Build script
├── Run.cmd        # Run script
├── screenshots/   # Example images
├── README.md
├── .gitignore
└── LICENSE
```

---

## 🔍 Features

- Visualization of the electrostatic field using vector arrows
- Adding and editing positive and negative point charges
- Probes that show field strength at their location over time (graph)
- Interactive scenarios – e.g., dynamically changing charge magnitudes
- Saving and loading custom scenes

---

## 🛠 Technologies

- C# (.NET Framework 4.7.2)
- Windows Forms (WinForms)
- System.Drawing, Charting

---

## ▶️ Running the Project

Open the solution file `src/ElectricFieldVis.sln` in Visual Studio and press `F5` to run.

Or run from terminal:
```bash
Build.cmd
Run.cmd
```

---

## 📸 Screenshots

| Field with probes and charges |
|-------------------------------|
| ![Screenshot 1](screenshots/field-1.jpg) |
| ![Screenshot 2](screenshots/field-2.jpg) |
| ![Screenshot 3](screenshots/field-3.jpg) |

---

## 📄 Documentation

For detailed information, see `doc/Dokumentace.pdf`.

---

## 📄 License

This project is licensed under the MIT License – see [LICENSE](LICENSE).
