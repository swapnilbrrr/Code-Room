using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Services;

/// <summary>
/// Creates the database schema when needed and seeds the baseline content and demonstration accounts.
/// The seed is intentionally idempotent so a fresh MySQL database can be prepared from the submitted project.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        await UpgradeLegacyDemoDataAsync(db);

        if (await db.Users.AnyAsync())
        {
            return;
        }

        SeedUsers(db);
        SeedCatalogue(db);
        SeedAnnouncements(db);

        await db.SaveChangesAsync();
    }

    private static async Task UpgradeLegacyDemoDataAsync(ApplicationDbContext db)
    {
        var demos = new (string[] Aliases, string Email, string Name, string Password, string Role)[]
        {
            (["superadmin@coderoom.test", "platform.admin@coderoom.test"], "swapnil.superadmin@coderoom.test", "Swapnil Katuwal", "Swapnil.Admin@2026", Roles.SuperAdmin),
            (["admin@coderoom.test", "content.manager@coderoom.test"], "chandra.admin@coderoom.test", "Chandra Shrestha", "Chandra.Admin@2026", Roles.Admin),
            (["student@coderoom.test", "aarav.learner@coderoom.test"], "bijay.student@coderoom.test", "Bijay Thapa", "Bijay.Student@2026", Roles.Student)
        };

        var changed = false;

        foreach (var demo in demos)
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                demo.Aliases.Contains(u.Email) || u.Email == demo.Email);

            if (user is null)
            {
                db.Users.Add(new User
                {
                    FullName = demo.Name,
                    Email = demo.Email,
                    PasswordHash = PasswordHasher.Hash(demo.Password),
                    Role = demo.Role
                });
                changed = true;
            }
            else
            {
                if (user.Email != demo.Email)
                {
                    user.Email = demo.Email;
                    changed = true;
                }

                if (user.FullName != demo.Name)
                {
                    user.FullName = demo.Name;
                    changed = true;
                }

                if (user.Role != demo.Role)
                {
                    user.Role = demo.Role;
                    changed = true;
                }

                // Keep the built-in demo account usable after an older seed/version.
                if (!PasswordHasher.Verify(demo.Password, user.PasswordHash))
                {
                    user.PasswordHash = PasswordHasher.Hash(demo.Password);
                    changed = true;
                }
            }
        }

        var additionalUsers = new (string[] Aliases, string Email, string Name, string Password)[]
        {
            (["anisha.student@coderoom.test", "anisha.gurung@coderoom.com"], "anisha.gurung@coderoom.com", "Anisha Gurung", "Anisha.Student@2026"),
            (["nischal.student@coderoom.test", "nischal.bhandari@coderoom.com"], "nischal.bhandari@coderoom.com", "Nischal Bhandari", "Nischal.Student@2026"),
            (["suman.student@coderoom.test", "suman.adhikari@coderoom.com"], "suman.adhikari@coderoom.com", "Suman Adhikari", "Suman.Student@2026"),
            (["prerana.student@coderoom.test", "prerana.rai@coderoom.com"], "prerana.rai@coderoom.com", "Prerana Rai", "Prerana.Student@2026"),
            (["babin.student@coderoom.test"], "babin.aryal@coderoom.com", "Babin Aryal", "Babin.Student@2026")
        };

        foreach (var demo in additionalUsers)
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                demo.Aliases.Contains(u.Email) || u.Email == demo.Email);

            if (user is null)
            {
                db.Users.Add(new User
                {
                    FullName = demo.Name,
                    Email = demo.Email,
                    PasswordHash = PasswordHasher.Hash(demo.Password),
                    Role = Roles.Student
                });
                changed = true;
                continue;
            }

            if (user.Email != demo.Email)
            {
                user.Email = demo.Email;
                changed = true;
            }

            if (user.FullName != demo.Name)
            {
                user.FullName = demo.Name;
                changed = true;
            }

            if (user.Role != Roles.Student)
            {
                user.Role = Roles.Student;
                changed = true;
            }

            if (!PasswordHasher.Verify(demo.Password, user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.Hash(demo.Password);
                changed = true;
            }
        }

        var lessons = await db.Lessons
            .Include(l => l.Course)
            .ToListAsync();

        foreach (var lesson in lessons)
        {
            if (lesson.Content.Trim().Length < 700)
            {
                lesson.Content = LessonContentBuilder.Build(
                    lesson.Course.Title,
                    lesson.Title,
                    lesson.Order);
                changed = true;
            }

            if (lesson.Order == 1 && string.IsNullOrWhiteSpace(lesson.VideoUrl))
            {
                lesson.VideoUrl = LessonMediaCatalog.VideoFor(lesson.Course.Title);
                changed = true;
            }

            if (lesson.Order == 1 && string.IsNullOrWhiteSpace(lesson.ResourceUrl))
            {
                lesson.ResourceUrl = LessonMediaCatalog.ResourceFor(lesson.Course.Title);
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }

    private static void SeedUsers(ApplicationDbContext db)
    {
        db.Users.AddRange(
            new User
            {
                FullName = "Swapnil Katuwal",
                Email = "swapnil.superadmin@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Swapnil.Admin@2026"),
                Role = Roles.SuperAdmin
            },
            new User
            {
                FullName = "Chandra Shrestha",
                Email = "chandra.admin@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Chandra.Admin@2026"),
                Role = Roles.Admin
            },
            new User
            {
                FullName = "Bijay Thapa",
                Email = "bijay.student@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Bijay.Student@2026"),
                Role = Roles.Student
            },
            new User
            {
                FullName = "Anisha Gurung",
                Email = "anisha.student@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Anisha.Student@2026"),
                Role = Roles.Student
            },
            new User
            {
                FullName = "Nischal Bhandari",
                Email = "nischal.student@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Nischal.Student@2026"),
                Role = Roles.Student
            },
            new User
            {
                FullName = "Suman Adhikari",
                Email = "suman.student@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Suman.Student@2026"),
                Role = Roles.Student
            },
            new User
            {
                FullName = "Prerana Rai",
                Email = "prerana.student@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Prerana.Student@2026"),
                Role = Roles.Student
            });
    }

    private static void SeedAnnouncements(ApplicationDbContext db)
    {
        db.Announcements.AddRange(
            new Announcement
            {
                Title = "Welcome to Code-Room",
                Message = "Explore our technology courses, track your progress and test yourself with quizzes.",
                PublishedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Announcement
            {
                Title = "New cybersecurity path available",
                Message = "Networking, Linux and Cybersecurity Foundations are now live in the catalogue.",
                PublishedAt = DateTime.UtcNow.AddDays(-2)
            });
    }

    private static void SeedCatalogue(ApplicationDbContext db)
    {
        foreach (var def in Definitions)
        {
            var course = new Course
            {
                Title = def.Title,
                Slug = def.Slug,
                Description = def.Description,
                Category = def.Category,
                Level = def.Level,
                IsPublished = true
            };

            for (var i = 0; i < def.Lessons.Length; i++)
            {
                course.Lessons.Add(new Lesson
                {
                    Title = def.Lessons[i],
                    Order = i + 1,
                    Content = LessonContentBuilder.Build(def.Title, def.Lessons[i], i + 1),
                    VideoUrl = i == 0 ? LessonMediaCatalog.VideoFor(def.Title) : null,
                    ResourceUrl = i == 0 ? LessonMediaCatalog.ResourceFor(def.Title) : null,
                    IsPublished = true
                });
            }

            db.Courses.Add(course);

            var quiz = new Quiz
            {
                Course = course,
                Title = def.QuizTitle,
                Description = $"Check your understanding of {def.Title}."
            };

            foreach (var q in def.Questions)
            {
                quiz.Questions.Add(new Question
                {
                    QuestionText = q.Text,
                    OptionA = q.A,
                    OptionB = q.B,
                    OptionC = q.C,
                    OptionD = q.D,
                    CorrectOption = q.Correct
                });
            }

            db.Quizzes.Add(quiz);
        }

        db.Resources.AddRange(
            new Resource { Title = "C# Coding Standards (PDF)", Url = "https://learn.microsoft.com/dotnet/csharp/", Type = "Document" },
            new Resource { Title = "MDN Web Docs", Url = "https://developer.mozilla.org/", Type = "Link" },
            new Resource { Title = "OWASP Top 10", Url = "https://owasp.org/www-project-top-ten/", Type = "Link" });
    }

    private sealed record QuestionSeed(string Text, string A, string B, string C, string D, string Correct);

    private sealed record CourseSeed(
        string Title,
        string Slug,
        string Description,
        string Category,
        string Level,
        string[] Lessons,
        string QuizTitle,
        QuestionSeed[] Questions);

    private static readonly CourseSeed[] Definitions =
    [
        new CourseSeed("C# Fundamentals", "csharp-fundamentals",
            "Learn C# syntax, control flow and object-oriented programming fundamentals.",
            "Programming", "Beginner",
            ["Introduction to C#", "Variables & Data Types", "Operators & Expressions", "Conditional Statements", "Loops", "Methods", "Arrays & Collections", "Classes & Objects", "Inheritance & Polymorphism", "Mini Project"],
            "C# Fundamentals Quiz",
            [
                new("Which keyword defines a class in C#?", "class", "struct", "def", "function", "A"),
                new("Which type stores whole numbers?", "string", "int", "bool", "double", "B"),
                new("What is the entry point of a C# console app?", "Start()", "Run()", "Main()", "Init()", "C"),
                new("Which loop runs at least once?", "for", "while", "foreach", "do-while", "D"),
                new("Which concept lets a class reuse another class's members?", "Inheritance", "Encapsulation", "Overloading", "Casting", "A")
            ]),
        new CourseSeed("Python Programming", "python-programming",
            "Build a practical Python foundation through syntax, data structures, functions and file handling.",
            "Programming", "Beginner",
            ["Python Basics", "Variables & Data Types", "Conditions", "Loops", "Functions", "Lists, Tuples & Dictionaries", "Modules", "File Handling", "Exceptions", "Mini Project"],
            "Python Programming Quiz",
            [
                new("How do you define a function in Python?", "func", "def", "function", "define", "B"),
                new("Which data structure uses key-value pairs?", "list", "tuple", "dictionary", "set", "C"),
                new("Which symbol starts a comment in Python?", "//", "#", "--", "/*", "B"),
                new("Which keyword handles exceptions?", "catch", "except", "rescue", "trap", "B"),
                new("Which is an immutable sequence?", "list", "dict", "set", "tuple", "D")
            ]),
        new CourseSeed("HTML & CSS Foundations", "html-css-foundations",
            "Learn how to structure accessible pages and build responsive layouts with HTML and CSS.",
            "Web Development", "Beginner",
            ["How the Web Works", "HTML Structure", "Semantic HTML", "CSS Fundamentals", "Box Model", "Flexbox", "Grid", "Responsive Design", "Forms & Accessibility", "Mini Project"],
            "HTML & CSS Quiz",
            [
                new("Which tag defines the largest heading?", "<h6>", "<head>", "<h1>", "<header>", "C"),
                new("Which CSS property changes text colour?", "font-color", "color", "text-style", "fill", "B"),
                new("Which layout module is one-dimensional?", "Grid", "Flexbox", "Table", "Float", "B"),
                new("Which tag is semantic for navigation?", "<div>", "<nav>", "<menu-bar>", "<links>", "B"),
                new("Which unit is relative to the root font size?", "px", "pt", "rem", "cm", "C")
            ]),
        new CourseSeed("ASP.NET Core MVC", "aspnet-core-mvc",
            "Understand MVC architecture, routing, Razor views, validation and data access in ASP.NET Core.",
            "Web Development", "Intermediate",
            ["Introduction to ASP.NET Core", "MVC Architecture", "Controllers", "Models", "Razor Views", "Routing", "Forms & Validation", "Entity Framework Core", "Authentication", "Building an MVC Application"],
            "ASP.NET Core MVC Quiz",
            [
                new("Which component handles incoming HTTP requests in MVC?", "Model", "View", "Controller", "Database", "C"),
                new("Which file configures the request pipeline in .NET 8?", "Startup.cs", "Program.cs", "Web.config", "App.cs", "B"),
                new("What does EF Core provide?", "Styling", "Object-relational mapping", "Routing", "Logging", "B"),
                new("Which attribute validates a required field?", "[Key]", "[Required]", "[Bind]", "[Route]", "B"),
                new("Razor view files use which extension?", ".razor", ".cshtml", ".html", ".vbhtml", "B")
            ]),
        new CourseSeed("Networking Fundamentals", "networking-fundamentals",
            "Build a practical foundation in network models, addressing, protocols and basic troubleshooting.",
            "Cybersecurity", "Beginner",
            ["What is a Network?", "OSI Model", "TCP/IP Model", "IPv4 & IPv6", "MAC & ARP", "TCP & UDP", "Ports & Protocols", "DNS", "HTTP/HTTPS", "Network Troubleshooting"],
            "Networking Fundamentals Quiz",
            [
                new("How many layers are in the OSI model?", "5", "6", "7", "8", "C"),
                new("Which protocol is connection-oriented?", "UDP", "TCP", "ICMP", "ARP", "B"),
                new("What does DNS resolve?", "IP to MAC", "Domain names to IP addresses", "Ports to services", "Files to folders", "B"),
                new("Which port does HTTPS use by default?", "21", "80", "443", "25", "C"),
                new("Which address is a private IPv4 range?", "8.8.8.8", "192.168.1.1", "172.15.0.1", "11.0.0.1", "B")
            ]),
        new CourseSeed("Linux Fundamentals", "linux-fundamentals",
            "Learn Linux filesystem navigation, permissions, processes, networking commands and basic security.",
            "Cybersecurity", "Beginner",
            ["Linux Basics", "Filesystem", "Navigation & CLI", "Users & Groups", "Permissions", "Processes", "Networking Commands", "Package Management", "Shell Basics", "Linux Security Basics"],
            "Linux Fundamentals Quiz",
            [
                new("Which command lists directory contents?", "ls", "cd", "pwd", "mv", "A"),
                new("Which command shows the current directory?", "dir", "pwd", "cwd", "loc", "B"),
                new("What does chmod change?", "Ownership", "Permissions", "Filename", "Size", "B"),
                new("Which file lists user accounts?", "/etc/passwd", "/etc/hosts", "/var/log", "/home", "A"),
                new("Which command displays running processes?", "jobs", "ps", "run", "top-list", "B")
            ]),
        new CourseSeed("Cybersecurity Foundations", "cybersecurity-foundations",
            "Explore core defensive security concepts, common threats, controls and incident response.",
            "Cybersecurity", "Intermediate",
            ["Introduction to Cybersecurity", "CIA Triad", "Threats & Vulnerabilities", "Authentication & Authorization", "Malware", "Phishing", "Firewalls", "Endpoint Security", "Security Monitoring", "Incident Response Basics"],
            "Cybersecurity Foundations Quiz",
            [
                new("What does the 'C' in the CIA triad stand for?", "Control", "Confidentiality", "Compliance", "Continuity", "B"),
                new("Which attack tricks users via fake emails?", "DDoS", "Phishing", "SQL Injection", "Spoofing", "B"),
                new("What does a firewall primarily do?", "Encrypt files", "Filter network traffic", "Back up data", "Scan disks", "B"),
                new("Which is a strong authentication factor combination?", "Password only", "Two-factor authentication", "Username only", "Email only", "B"),
                new("What is the first phase of incident response?", "Recovery", "Preparation", "Eradication", "Reporting", "B")
            ]),
        new CourseSeed("Database Fundamentals", "database-fundamentals",
            "Understand relational databases, SQL operations, relationships, constraints and basic design.",
            "Databases", "Beginner",
            ["Introduction to Databases", "Relational Databases", "Tables & Relationships", "Primary & Foreign Keys", "SQL SELECT", "INSERT / UPDATE / DELETE", "JOINs", "Constraints", "Normalization", "Database Design"],
            "Database Fundamentals Quiz",
            [
                new("Which SQL statement retrieves data?", "GET", "SELECT", "FETCH", "READ", "B"),
                new("What uniquely identifies a row in a table?", "Foreign key", "Primary key", "Index", "View", "B"),
                new("Which clause filters rows in a query?", "ORDER BY", "WHERE", "GROUP BY", "HAVING", "B"),
                new("Which JOIN returns only matching rows?", "LEFT JOIN", "RIGHT JOIN", "INNER JOIN", "FULL JOIN", "C"),
                new("What does normalization reduce?", "Speed", "Data redundancy", "Security", "Storage cost only", "B")
            ]),
    ];
}
