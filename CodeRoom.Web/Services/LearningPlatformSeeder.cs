using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Services;

public static class LearningPlatformSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await UpgradeNetworkingCourseAsync(db);
        await SeedExpandedCoursesAsync(db);
        await EnsureModulesForAllCoursesAsync(db);
        await SeedAchievementsAsync(db);
        await SeedResourceLinksAsync(db);
        await MaterializeLessonResourcesAsync(db);
    }

    private static async Task UpgradeNetworkingCourseAsync(ApplicationDbContext db)
    {
        var course = await db.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Slug == "networking-fundamentals");

        if (course is null)
        {
            return;
        }

        course.Title = "CCNA-Style Networking & Exam Prep";
        course.Slug = "ccna-style-networking-exam-prep";
        course.Description = "A practical, exam-focused networking path covering IPv4, switching, routing, VLANs, wireless, security and troubleshooting. This is an independent CCNA-style study path, not an official Cisco course.";
        course.Category = "Networking";
        course.Level = "Intermediate";
        course.EstimatedMinutes = 420;
        course.IsCertification = true;
        course.CertificateName = "Code-Room Networking Foundations Certificate";
        course.PassingScorePercent = 70;

        var ccnaLessons = new[]
        {
            "Network Fundamentals & Topologies",
            "IPv4 Addressing & Subnetting",
            "Ethernet, Switching & VLANs",
            "Routing & Static Routes",
            "Wireless, NAT & Network Services",
            "Access Control & Network Security",
            "Troubleshooting with CLI Tools",
            "Exam Scenarios & Packet Reasoning"
        };

        for (var i = 0; i < Math.Min(course.Lessons.Count, ccnaLessons.Length); i++)
        {
            course.Lessons.OrderBy(l => l.Order).ElementAt(i).Title = ccnaLessons[i];
            course.Lessons.OrderBy(l => l.Order).ElementAt(i).Summary = "Core concept, worked example, practical checks and exam-style reasoning.";
            course.Lessons.OrderBy(l => l.Order).ElementAt(i).Content = LessonContentBuilder.Build(course.Title, ccnaLessons[i], i + 1);
            course.Lessons.OrderBy(l => l.Order).ElementAt(i).ContentType = i % 3 == 0 ? "Video" : "Reading";
            course.Lessons.OrderBy(l => l.Order).ElementAt(i).DurationMinutes = 30 + (i * 3);
            course.Lessons.OrderBy(l => l.Order).ElementAt(i).VideoUrl = i < 2 ? "https://www.youtube.com/embed/QKfk7YFILws" : null;
        }

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.CourseId == course.Id);
        if (quiz is not null)
        {
            quiz.Title = "Networking Foundations Final Examination";
            quiz.AssessmentType = "Final Exam";
            quiz.TimeLimitMinutes = 30;
            quiz.PassingScorePercent = 70;
            quiz.IsCertificationExam = true;
        }
    }

    private static async Task SeedExpandedCoursesAsync(ApplicationDbContext db)
    {
        foreach (var def in Definitions)
        {
            var course = await db.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Slug == def.Slug);

            if (course is null)
            {
                course = new Course
                {
                    Title = def.Title,
                    Slug = def.Slug,
                    Description = def.Description,
                    Category = def.Category,
                    Level = def.Level,
                    EstimatedMinutes = def.Minutes,
                    IsCertification = def.Certification,
                    CertificateName = def.CertificateName,
                    PassingScorePercent = def.PassScore,
                    IsPublished = true
                };
                db.Courses.Add(course);
                await db.SaveChangesAsync();
                await db.Entry(course).Collection(c => c.Lessons).LoadAsync();
                await db.Entry(course).Collection(c => c.Modules).LoadAsync();
            }
            else
            {
                course.Description = def.Description;
                course.Category = def.Category;
                course.Level = def.Level;
                course.EstimatedMinutes = def.Minutes;
                course.IsCertification = def.Certification;
                course.CertificateName = def.CertificateName;
                course.PassingScorePercent = def.PassScore;
                course.IsPublished = true;
            }

            if (course.Lessons.Count == 0)
            {
                for (var i = 0; i < def.Lessons.Length; i++)
                {
                    var lesson = def.Lessons[i];
                    course.Lessons.Add(new Lesson
                    {
                        Course = course,
                        Title = lesson.Title,
                        Summary = lesson.Summary,
                        Content = LessonContentBuilder.Build(course.Title, lesson.Title, i + 1),
                        ContentType = lesson.Type,
                        VideoUrl = lesson.Video,
                        ResourceUrl = lesson.Resource,
                        DurationMinutes = lesson.Duration,
                        Order = i + 1,
                        IsPublished = true
                    });
                }
            }

            if (course.Modules.Count == 0)
            {
                for (var moduleIndex = 0; moduleIndex < Math.Ceiling(course.Lessons.Count / 2.0); moduleIndex++)
                {
                    course.Modules.Add(new CourseModule
                    {
                        Course = course,
                        Title = $"Module {moduleIndex + 1} — {GetModuleTitle(def.Category, moduleIndex)}",
                        Description = $"Learn and practise the {GetModuleTitle(def.Category, moduleIndex).ToLowerInvariant()} stage of this path.",
                        Order = moduleIndex + 1
                    });
                }
            }

            var orderedModules = course.Modules.OrderBy(m => m.Order).ToList();
            var orderedLessons = course.Lessons.OrderBy(l => l.Order).ToList();
            for (var i = 0; i < orderedLessons.Count; i++)
            {
                if (orderedLessons[i].CourseModuleId is null && orderedModules.Count > 0)
                {
                    orderedLessons[i].CourseModule = orderedModules[Math.Min(i / 2, orderedModules.Count - 1)];
                }

                if (string.IsNullOrWhiteSpace(orderedLessons[i].Summary))
                {
                    orderedLessons[i].Summary = "Concept walkthrough, worked example and practice checkpoint.";
                }

                if (orderedLessons[i].Content.StartsWith("Lesson ", StringComparison.OrdinalIgnoreCase))
                {
                    orderedLessons[i].Content = LessonContentBuilder.Build(course.Title, orderedLessons[i].Title, orderedLessons[i].Order);
                }
            }

            var quiz = await db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.CourseId == course.Id);

            if (quiz is null)
            {
                quiz = new Quiz
                {
                    Course = course,
                    Title = def.Certification ? $"{course.Title} Final Exam" : $"{course.Title} Knowledge Check",
                    Description = $"Test your understanding of {course.Title}.",
                    AssessmentType = def.Certification ? "Final Exam" : "Quiz",
                    TimeLimitMinutes = def.Certification ? 30 : 0,
                    PassingScorePercent = def.PassScore,
                    IsCertificationExam = def.Certification
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
            else if (quiz.Questions.Count == 0)
            {
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
            }

            if (def.Challenges.Length > 0 && !await db.Challenges.AnyAsync(c => c.CourseId == course.Id))
            {
                foreach (var challenge in def.Challenges)
                {
                    db.Challenges.Add(new Challenge
                    {
                        Course = course,
                        Lesson = course.Lessons.OrderBy(l => l.Order).Skip(Math.Min(challenge.LessonOffset, course.Lessons.Count - 1)).FirstOrDefault(),
                        Title = challenge.Title,
                        Instructions = challenge.Instructions,
                        StarterCode = challenge.StarterCode,
                        Hint = challenge.Hint,
                        ExpectedAnswer = challenge.ExpectedAnswer,
                        ValidationMode = challenge.Mode,
                        Points = challenge.Points
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAchievementsAsync(ApplicationDbContext db)
    {
        var definitions = new[]
        {
            ("first-course", "First Course", "Enrol in your first learning path.", "🧭", 30),
            ("first-lesson", "First Steps", "Complete your first lesson.", "🚀", 25),
            ("quiz-starter", "Quiz Starter", "Submit your first quiz.", "🧠", 30),
            ("challenge-starter", "Challenge Accepted", "Complete your first practice challenge.", "⚡", 50),
            ("course-finisher", "Course Finisher", "Complete your first full course.", "🏆", 100),
            ("streak-7", "Week on Fire", "Maintain a 7-day learning streak.", "🔥", 100),
            ("xp-500", "Five Hundred Club", "Reach 500 XP.", "💎", 150),
            ("certificate", "Certified Learner", "Earn your first Code-Room certificate.", "🎓", 200),
            ("cloud-path", "Cloud Explorer", "Enroll in a cloud learning path.", "☁️", 50)
        };

        foreach (var def in definitions)
        {
            if (!await db.Achievements.AnyAsync(a => a.Code == def.Item1))
            {
                db.Achievements.Add(new Achievement
                {
                    Code = def.Item1,
                    Name = def.Item2,
                    Description = def.Item3,
                    Icon = def.Item4,
                    XpReward = def.Item5
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedResourceLinksAsync(ApplicationDbContext db)
    {
        foreach (var def in Resources)
        {
            var exists = await db.Resources.AnyAsync(r => r.Title == def.Title);
            if (exists)
            {
                continue;
            }

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == def.Slug);
            db.Resources.Add(new Resource
            {
                CourseId = course?.Id,
                Title = def.Title,
                Url = def.Url,
                Type = def.Type
            });
        }

        await db.SaveChangesAsync();
    }

    private static string GetModuleTitle(string category, int index) => category switch
    {
        "Cloud" => new[] { "Cloud concepts", "Core services", "Architecture & security", "Exam practice" }[Math.Min(index, 3)],
        "Programming" => new[] { "Foundations", "Core language", "Practical development", "Practice & projects" }[Math.Min(index, 3)],
        "Linux" => new[] { "CLI foundations", "System administration", "Automation", "Security & troubleshooting" }[Math.Min(index, 3)],
        "Cybersecurity" => new[] { "Foundations", "Defensive controls", "Monitoring", "Incident response & practice" }[Math.Min(index, 3)],
        "Networking" => new[] { "Network fundamentals", "Switching & routing", "Services & security", "Troubleshooting & exam practice" }[Math.Min(index, 3)],
        _ => new[] { "Core concepts", "Applied skills", "Practice", "Assessment" }[Math.Min(index, 3)]
    };

    private sealed record QuestionSeed(string Text, string A, string B, string C, string D, string Correct);

    private sealed record ChallengeSeed(string Title, string Instructions, string? StarterCode, string? Hint, string ExpectedAnswer, string Mode, int Points, int LessonOffset);

    private sealed record LessonSeed(string Title, string Summary, string Type, string? Video, string? Resource, int Duration);

    private sealed record CourseSeed(
        string Title,
        string Slug,
        string Description,
        string Category,
        string Level,
        int Minutes,
        bool Certification,
        string? CertificateName,
        int PassScore,
        LessonSeed[] Lessons,
        QuestionSeed[] Questions,
        ChallengeSeed[] Challenges);

    private sealed record ResourceSeed(string Slug, string Title, string Url, string Type);

    private static readonly CourseSeed[] Definitions =
    [
        new("Python Automation", "python-automation", "Automate repetitive tasks with Python, files, APIs, parsing and safe command execution patterns.", "Programming", "Intermediate", 300, false, null, 70,
        [
            new("Automation mindset", "Choose tasks that benefit from repeatable, testable scripts.", "Reading", "https://www.youtube.com/embed/rfscVS0vtbw", "https://docs.python.org/3/library/", 25),
            new("Files and directories", "Work with paths and files using pathlib and context managers.", "Video", null, "https://docs.python.org/3/library/pathlib.html", 30),
            new("CSV and JSON", "Parse structured data and produce useful outputs.", "Reading", null, "https://docs.python.org/3/library/json.html", 30),
            new("HTTP APIs", "Send requests, inspect responses and handle failures safely.", "Video", null, "https://developer.mozilla.org/en-US/docs/Web/HTTP", 35),
            new("Logging and errors", "Make automation scripts observable and debuggable.", "Reading", null, "https://docs.python.org/3/library/logging.html", 30),
            new("Automation mini-project", "Combine input, processing, output and logging in one small tool.", "Challenge", null, null, 45)
        ],
        [
            new("Which Python module is designed for filesystem paths?", "path", "pathlib", "filepaths", "ospath", "B"),
            new("Which format is commonly used for structured API data?", "JSON", "BMP", "WAV", "EXE", "A"),
            new("What should automation scripts do when an external request fails?", "Ignore it", "Crash silently", "Handle and report the failure", "Retry forever", "C"),
            new("Which module provides application logging?", "logging", "debug", "trace", "report", "A"),
            new("What is a good automation property?", "Repeatable", "Random", "Hidden", "Unlogged", "A")
        ],
        [
            new("Normalize a file extension", "Return the lowercase extension for a filename such as report.CSV.", "filename = 'report.CSV'", "Remember the extension includes the dot.", ".csv", "Contains", 50, 1)
        ]),
        new("Python for Cybersecurity", "python-for-cybersecurity", "Use Python to reason about security logs, indicators, sockets, parsing and defensive automation.", "Cybersecurity", "Intermediate", 330, false, null, 70,
        [
            new("Python in defensive security", "Understand where scripting accelerates repetitive analyst tasks.", "Video", "https://www.youtube.com/embed/rfscVS0vtbw", "https://docs.python.org/3/", 25),
            new("Log parsing", "Extract timestamps, usernames, IPs and event fields from raw logs.", "Reading", null, "https://docs.python.org/3/library/re.html", 35),
            new("IP and port validation", "Validate network indicators before using them in analysis.", "Reading", null, "https://docs.python.org/3/library/ipaddress.html", 30),
            new("Socket basics", "Understand client/server communication and safe socket experiments.", "Video", null, "https://docs.python.org/3/library/socket.html", 35),
            new("Threat intel enrichment", "Normalise indicators and prepare data for enrichment workflows.", "Reading", null, "https://www.cisa.gov/topics/cyber-threats-and-advisories", 35),
            new("SOC triage mini-project", "Turn a small log set into a structured triage summary.", "Challenge", null, null, 45)
        ],
        [
            new("Which Python module validates IP addresses?", "socket", "ipaddress", "network", "netaddr", "B"),
            new("Why parse logs into fields?", "To remove evidence", "To make analysis consistent", "To hide timestamps", "To slow investigations", "B"),
            new("What should be done before enriching an indicator?", "Normalise and validate it", "Delete it", "Publish it", "Encrypt the hostname only", "A"),
            new("Which data is especially useful in SOC triage?", "Timestamp and source", "Font size", "Screen brightness", "Wallpaper", "A"),
            new("What does a socket provide?", "A communication endpoint", "A database table", "A password vault", "A web template", "A")
        ],
        [
            new("Extract an IP from text", "Return the IP address from 'source=10.10.10.5'.", "text = 'source=10.10.10.5'", "Keep the answer as the IPv4 address only.", "10.10.10.5", "Exact", 50, 1),
            new("Count failed logins", "Return the count of the word 'failed' in a short log string.", "log = 'failed ok failed'", null, "2", "Exact", 50, 2)
        ]),
        new("Bash Fundamentals", "bash-fundamentals", "Master Bash navigation, variables, pipes, redirection, loops and defensive shell habits.", "Linux", "Beginner", 280, false, null, 70,
        [
            new("Shell and terminal basics", "Understand commands, prompts, paths and exit codes.", "Video", "https://www.youtube.com/embed/pkZEKIXe3u4", "https://www.gnu.org/software/bash/manual/", 25),
            new("Variables and quoting", "Use variables safely and understand quoting rules.", "Reading", null, "https://www.gnu.org/software/bash/manual/bash.html#Shell-Parameters", 30),
            new("Pipes and redirection", "Build useful command pipelines and capture output.", "Video", null, "https://www.gnu.org/software/bash/manual/bash.html#Pipelines", 30),
            new("Conditions and loops", "Automate repeated shell operations with checks.", "Reading", null, "https://www.gnu.org/software/bash/manual/bash.html#Conditional-Constructs", 35),
            new("Permissions and processes", "Inspect permissions and running processes from the shell.", "Reading", null, "https://man7.org/linux/man-pages/", 35),
            new("Bash automation challenge", "Combine variables, loops and command output in a safe mini-script.", "Challenge", null, null, 40)
        ],
        [
            new("Which command prints the current directory?", "cd", "pwd", "ls", "dir", "B"),
            new("Which symbol redirects standard output?", ">", "|", "&", "#", "A"),
            new("Which command lists files?", "pwd", "ls", "mkdir", "whoami", "B"),
            new("Why quote variables?", "To preserve intended argument boundaries", "To make them random", "To disable scripts", "To hide output", "A"),
            new("Which command changes file permissions?", "chown", "chmod", "ps", "grep", "B")
        ],
        [
            new("Print the working directory", "Give the Bash command that prints the current working directory.", null, null, "pwd", "Exact", 30, 0)
        ]),
        new("AWS Cloud Essentials", "aws-cloud-essentials", "Build an exam-oriented AWS foundation around cloud concepts, core services, IAM, networking, security and billing.", "Cloud", "Beginner", 360, true, "Code-Room AWS Cloud Essentials Certificate", 70,
        [
            new("Cloud concepts", "Understand elasticity, regions, availability zones and shared responsibility.", "Video", "https://www.youtube.com/embed/3hLmDS179YE", "https://aws.amazon.com/what-is-cloud-computing/", 35),
            new("AWS compute", "Compare EC2, Lambda and container-oriented workloads.", "Video", null, "https://aws.amazon.com/ec2/", 40),
            new("AWS storage and databases", "Choose between S3, EBS, RDS and other storage patterns.", "Reading", null, "https://aws.amazon.com/products/storage/", 40),
            new("IAM and security", "Apply least privilege, MFA and role-based access.", "Video", null, "https://docs.aws.amazon.com/IAM/latest/UserGuide/introduction.html", 45),
            new("VPC networking", "Reason about subnets, routing, security groups and network boundaries.", "Reading", null, "https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html", 45),
            new("Cloud billing & exam practice", "Use pricing concepts and exam-style scenario questions.", "Assessment", null, "https://aws.amazon.com/pricing/", 35)
        ],
        [
            new("Which AWS service is object storage?", "EC2", "S3", "RDS", "Lambda", "B"),
            new("What does IAM primarily control?", "Identity and permissions", "Video encoding", "DNS only", "CPU scheduling", "A"),
            new("Which service runs code without managing servers directly?", "Lambda", "EBS", "VPC", "Route 53", "A"),
            new("What is a security group?", "A stateful virtual firewall for resources", "A billing invoice", "A storage bucket", "A DNS record", "A"),
            new("Which AWS concept isolates failures geographically?", "Availability Zones", "Tags", "Accounts only", "Queues", "A")
        ],
        [
            new("Choose object storage", "Return the AWS service used for object storage.", null, null, "S3", "Exact", 40, 0)
        ]),
        new("Azure Cloud Fundamentals", "azure-cloud-fundamentals", "Explore Azure services, resource groups, identity, networking, storage, monitoring and cloud security.", "Cloud", "Beginner", 330, true, "Code-Room Azure Cloud Foundations Certificate", 70,
        [
            new("Azure fundamentals", "Understand subscriptions, regions, availability zones and resource groups.", "Reading", null, "https://learn.microsoft.com/azure/cloud-adoption-framework/", 35),
            new("Compute services", "Compare virtual machines, App Service, containers and serverless options.", "Video", null, "https://learn.microsoft.com/azure/architecture/guide/technology-choices/compute-decision-tree", 40),
            new("Storage", "Understand Blob, Files and managed storage patterns.", "Reading", null, "https://learn.microsoft.com/azure/storage/common/storage-introduction", 35),
            new("Microsoft Entra ID", "Learn cloud identity, roles and least privilege.", "Video", null, "https://learn.microsoft.com/entra/fundamentals/whatis", 40),
            new("Virtual networking", "Reason about VNets, subnets and network security.", "Reading", null, "https://learn.microsoft.com/azure/virtual-network/virtual-networks-overview", 45),
            new("Azure security & exam practice", "Apply shared responsibility and scenario-based reasoning.", "Assessment", null, "https://learn.microsoft.com/security/", 35)
        ],
        [
            new("What groups Azure resources logically?", "Resource groups", "Availability Zones", "VNets", "Tenants only", "A"),
            new("Which service provides cloud identity?", "Microsoft Entra ID", "Blob Storage", "VM Scale Sets", "Azure DNS", "A"),
            new("What is a VNet?", "A virtual network boundary", "A database engine", "A billing plan", "A user account", "A"),
            new("Blob Storage is primarily for what?", "Object data", "CPU scheduling", "Identity tokens", "DNS", "A"),
            new("What principle should guide cloud permissions?", "Least privilege", "Everyone admin", "Shared passwords", "Permanent access", "A")
        ],
        [
            new("Name the Azure identity service", "Provide the current Microsoft cloud identity service name.", null, null, "Microsoft Entra ID", "Contains", 40, 3)
        ]),
        new("Cloud Security Fundamentals", "cloud-security-fundamentals", "Apply IAM, network segmentation, logging, shared responsibility and secure cloud architecture principles.", "Cloud", "Intermediate", 300, false, null, 70,
        [
            new("Shared responsibility", "Separate provider responsibilities from customer responsibilities.", "Reading", null, "https://cloud.google.com/learn/what-is-cloud-computing", 30),
            new("Cloud IAM", "Apply role-based access, MFA and least privilege.", "Video", null, "https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html", 35),
            new("Network controls", "Use segmentation, security groups and private connectivity concepts.", "Reading", null, "https://learn.microsoft.com/security/benchmark/azure/introduction", 35),
            new("Logging and monitoring", "Design useful audit trails and alert signals.", "Video", null, "https://aws.amazon.com/cloudtrail/", 35),
            new("Secrets and data protection", "Protect keys, secrets and sensitive information at rest and in transit.", "Reading", null, "https://owasp.org/www-project-top-ten/", 35),
            new("Cloud incident response", "Use evidence and containment thinking in cloud incidents.", "Challenge", null, null, 40)
        ],
        [
            new("Which principle limits excessive permissions?", "Least privilege", "Shared admin", "Default public", "Flat access", "A"),
            new("Why centralise cloud logs?", "For investigation and accountability", "To hide attacks", "To reduce timestamps", "To remove evidence", "A"),
            new("What should secrets be stored in?", "A managed secret store", "Source code", "Public README", "Browser local storage", "A"),
            new("Which control reduces lateral movement?", "Network segmentation", "Shared passwords", "Open security groups", "Public buckets", "A"),
            new("What should incident response preserve?", "Evidence", "Only screenshots", "Nothing", "Credentials", "A")
        ],
        [
            new("Identify the security principle", "Name the principle that says a user should receive only the access needed for the task.", null, null, "least privilege", "Contains", 50, 1)
        ]),
        new("SOC Analyst Foundations", "soc-analyst-foundations", "Learn alert triage, log analysis, detection thinking, investigation workflow and incident documentation.", "Cybersecurity", "Intermediate", 340, true, "Code-Room SOC Analyst Foundations Certificate", 70,
        [
            new("SOC workflow", "Understand triage, scoping, escalation and documentation.", "Reading", null, "https://attack.mitre.org/", 30),
            new("Windows and Linux logs", "Identify useful authentication, process and system events.", "Video", null, "https://learn.microsoft.com/windows/security/threat-protection/auditing/basic-audit-policy-settings", 40),
            new("Network evidence", "Read IPs, ports, protocols and packet context.", "Video", "https://www.youtube.com/embed/7_G0cY9Y5V8", "https://www.wireshark.org/docs/", 40),
            new("Detection and false positives", "Separate suspicious signals from benign activity using context.", "Reading", null, "https://attack.mitre.org/matrices/enterprise/", 35),
            new("Incident response notes", "Document who, what, when, where and why.", "Reading", null, "https://csrc.nist.gov/projects/incident-response", 35),
            new("SOC case simulation", "Complete an analyst-style alert investigation and assessment.", "Assessment", null, null, 45)
        ],
        [
            new("What is alert triage?", "Initial analysis and prioritisation", "Deleting alerts", "Patching servers", "Writing code only", "A"),
            new("Why correlate multiple log sources?", "To add context", "To remove evidence", "To create noise", "To avoid timelines", "A"),
            new("What is a false positive?", "A benign event incorrectly alerted as suspicious", "A confirmed attack", "A deleted log", "A blocked port", "A"),
            new("Which field anchors an investigation timeline?", "Timestamp", "Font", "Window size", "Theme", "A"),
            new("What should an analyst document?", "Evidence and actions", "Only their opinion", "Passwords", "Nothing", "A")
        ],
        [
            new("Name the first triage question", "Provide the investigation question that identifies when the event occurred.", null, null, "when", "Contains", 35, 1)
        ]),
        new("Web Application Security", "web-application-security", "Practise secure web development using authentication, validation, session protection, XSS, CSRF and common web attack concepts.", "Cybersecurity", "Intermediate", 320, false, null, 70,
        [
            new("Web attack surface", "Map inputs, sessions, APIs and trust boundaries.", "Reading", null, "https://owasp.org/www-project-top-ten/", 30),
            new("Authentication & sessions", "Protect identity, cookies and session state.", "Video", null, "https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html", 40),
            new("Input validation & XSS", "Treat user input as untrusted and encode output correctly.", "Reading", null, "https://owasp.org/www-community/attacks/xss/", 40),
            new("CSRF", "Understand browser-based request forgery and anti-forgery tokens.", "Reading", null, "https://owasp.org/www-community/attacks/csrf", 35),
            new("SQL injection", "Understand why parameterised database access matters.", "Video", null, "https://owasp.org/www-community/attacks/SQL_Injection", 40),
            new("Secure coding review", "Review a small MVC flow and identify security improvements.", "Challenge", null, null, 45)
        ],
        [
            new("What does XSS target?", "Browser-executed script injection", "Database backup", "DNS caching", "CPU load", "A"),
            new("What protects state-changing forms from CSRF?", "Anti-forgery tokens", "Long URLs", "HTML comments", "More CSS", "A"),
            new("What helps prevent SQL injection?", "Parameterised queries", "String concatenation", "Public SQL", "Client-only validation", "A"),
            new("Should browser input be trusted?", "No", "Always", "Only at night", "Only on localhost", "A"),
            new("What is authentication?", "Verifying identity", "Granting every permission", "Rendering HTML", "Logging out", "A")
        ],
        [
            new("Choose the safer database access pattern", "Name the approach that binds user input as parameters instead of concatenating SQL strings.", null, null, "parameterised query", "Contains", 50, 4)
        ]),
        new("SQL & MySQL Foundations", "sql-mysql-foundations", "Learn relational modelling, SQL CRUD, joins, constraints, indexing and application database thinking.", "Databases", "Beginner", 300, false, null, 70,
        [
            new("Relational thinking", "Model entities, keys and relationships.", "Reading", null, "https://dev.mysql.com/doc/refman/8.0/en/", 30),
            new("SELECT & filtering", "Retrieve exactly the rows an application needs.", "Video", "https://www.youtube.com/embed/HXV3zeQKqGY", "https://dev.mysql.com/doc/refman/8.0/en/select.html", 35),
            new("INSERT UPDATE DELETE", "Perform database CRUD carefully and safely.", "Reading", null, "https://dev.mysql.com/doc/refman/8.0/en/insert.html", 35),
            new("JOINs", "Combine related data across tables.", "Reading", null, "https://dev.mysql.com/doc/refman/8.0/en/join.html", 35),
            new("Constraints & indexes", "Protect data integrity and improve common lookups.", "Video", null, "https://dev.mysql.com/doc/refman/8.0/en/create-table-foreign-keys.html", 35),
            new("Database design challenge", "Turn a learning requirement into a small relational schema.", "Challenge", null, null, 45)
        ],
        [
            new("Which SQL command reads rows?", "SELECT", "READ", "GET", "FETCHALL", "A"),
            new("What uniquely identifies a row?", "Primary key", "Foreign key", "Comment", "Trigger only", "A"),
            new("Which JOIN keeps only matching rows?", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "CROSS JOIN", "A"),
            new("What do foreign keys enforce?", "Relationships", "CSS", "Passwords", "File permissions", "A"),
            new("Why use indexes?", "Faster common lookups", "To remove data", "To encrypt rows", "To replace keys", "A")
        ],
        [
            new("Write a filter clause", "Return the SQL clause used to filter rows by a condition.", null, null, "WHERE", "Exact", 35, 1)
        ]),
        new("Certification Practice: Python", "certification-practice-python", "Exam-style Python practice course with timed assessment preparation, scenario questions and a certificate path.", "Programming", "Intermediate", 260, true, "Code-Room Python Foundations Certificate", 70,
        [
            new("Python exam map", "Review syntax, functions, collections, exceptions and files.", "Reading", "https://www.youtube.com/embed/rfscVS0vtbw", "https://docs.python.org/3/tutorial/", 30),
            new("Functions & collections", "Practise core Python patterns likely to appear in assessments.", "Video", null, "https://docs.python.org/3/tutorial/controlflow.html", 35),
            new("Errors & files", "Review exception handling and safe file workflows.", "Reading", null, "https://docs.python.org/3/tutorial/errors.html", 30),
            new("Data processing", "Combine dictionaries, lists and comprehensions in practical tasks.", "Reading", null, "https://docs.python.org/3/tutorial/datastructures.html", 30),
            new("Timed practice set", "Attempt scenario questions under a countdown.", "Assessment", null, null, 35),
            new("Final certification examination", "Complete the final assessment to qualify for a certificate.", "Final Exam", null, null, 60)
        ],
        [
            new("Function keyword", "def", "class", "func", "lambda-only", "A"),
            new("Immutable sequence", "list", "dict", "tuple", "set-only", "C"),
            new("Exception handler keyword", "catch", "except", "handle", "rescue", "B"),
            new("Dictionary access uses", "keys", "indexes only", "key-value lookup", "row ids", "C"),
            new("Which function returns the number of items?", "size", "len", "countall", "items", "B")
        ],
        [
            new("Return a Python list length", "Give the expression that returns the number of items in [1, 2, 3].", null, null, "len([1,2,3])", "Contains", 50, 2)
        ]),
        new("Certification Practice: Cloud", "certification-practice-cloud", "Cross-cloud certification practice covering core concepts, identity, networking, security and scenario reasoning.", "Cloud", "Intermediate", 300, true, "Code-Room Cloud Foundations Certificate", 70,
        [
            new("Cloud exam map", "Compare service models, regions, availability and shared responsibility.", "Video", "https://www.youtube.com/embed/3hLmDS179YE", "https://aws.amazon.com/what-is-cloud-computing/", 35),
            new("Identity & access", "Practise cloud IAM and least privilege scenarios.", "Reading", null, "https://learn.microsoft.com/security/zero-trust/develop/identity", 35),
            new("Networking & compute", "Compare virtual networks, subnets and compute choices.", "Reading", null, "https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html", 40),
            new("Storage & databases", "Match workloads to cloud storage and database services.", "Video", null, "https://aws.amazon.com/products/storage/", 35),
            new("Security scenarios", "Work through shared responsibility and incident examples.", "Assessment", null, "https://cloudsecurityalliance.org/", 40),
            new("Final cloud certification examination", "Complete the final cross-cloud assessment.", "Final Exam", null, null, 60)
        ],
        [
            new("Object storage example", "S3", "EC2", "VPC", "IAM", "A"),
            new("Identity principle", "Least privilege", "Public access", "Shared password", "Permanent root", "A"),
            new("Network segmentation uses", "Subnets", "Fonts", "Users only", "PDFs", "A"),
            new("Shared responsibility means", "Security duties are divided by layer", "Provider does everything", "Customer does nothing", "No security needed", "A"),
            new("Serverless compute example", "Lambda", "S3", "EBS", "VPC", "A")
        ],
        [
            new("Name the least privilege principle", "State the cloud security principle for granting only required permissions.", null, null, "least privilege", "Contains", 40, 1)
        ]),
        new("Certification Practice: Networking", "certification-practice-networking", "Exam preparation for routing, switching, subnetting, services and network troubleshooting scenarios.", "Networking", "Intermediate", 320, true, "Code-Room Networking Foundations Certificate", 70,
        [
            new("Subnetting practice", "Convert prefixes to usable address ranges.", "Reading", "https://www.youtube.com/embed/QKfk7YFILws", "https://www.cisco.com/c/en/us/support/docs/ip/routing-information-protocol-rip/13788-3.html", 45),
            new("Switching & VLANs", "Practise Ethernet forwarding, VLANs and trunk concepts.", "Video", null, "https://www.cisco.com/c/en/us/support/docs/lan-switching/virtual-lans-vlan/10023-3.html", 40),
            new("Routing", "Compare connected, static and dynamic routing ideas.", "Reading", null, "https://www.cisco.com/c/en/us/support/docs/ip/enhanced-interior-gateway-routing-protocol-eigrp/16406-eigrp-toc.html", 45),
            new("Network services", "Review DNS, DHCP, NAT and common transport ports.", "Reading", null, "https://www.cisco.com/c/en/us/support/docs/ip/domain-name-system-dns/116167-technote-dns-00.html", 40),
            new("Troubleshooting", "Use ping, traceroute, ARP, routes and interface evidence.", "Video", null, "https://www.cisco.com/c/en/us/support/docs/ip/ip-addressing-services/13788-3.html", 40),
            new("Final networking examination", "Solve scenario-based questions and subnetting checks.", "Final Exam", null, null, 60)
        ],
        [
            new("OSI layers", "5", "6", "7", "8", "C"),
            new("TCP is", "Connection-oriented", "Connectionless", "Only encrypted", "A routing protocol", "A"),
            new("HTTPS default port", "21", "80", "443", "25", "C"),
            new("Private address", "8.8.8.8", "192.168.1.10", "1.1.1.1", "9.9.9.9", "B"),
            new("DNS resolves", "Names to addresses", "MACs to passwords", "Ports to files", "Users to groups", "A")
        ],
        [
            new("Private IPv4 address", "Return a private IPv4 address from a common RFC1918 range.", null, null, "192.168.1.10", "Exact", 40, 1)
        ])
    ];

    private static readonly ResourceSeed[] Resources =
    [
        new("python-automation", "Python Standard Library Reference", "https://docs.python.org/3/library/", "Documentation"),
        new("python-for-cybersecurity", "Python ipaddress Module", "https://docs.python.org/3/library/ipaddress.html", "Documentation"),
        new("bash-fundamentals", "GNU Bash Reference", "https://www.gnu.org/software/bash/manual/", "Documentation"),
        new("aws-cloud-essentials", "AWS Skill Builder", "https://skillbuilder.aws/", "Learning"),
        new("azure-cloud-fundamentals", "Microsoft Learn — Azure", "https://learn.microsoft.com/training/azure/", "Learning"),
        new("cloud-security-fundamentals", "OWASP Top 10", "https://owasp.org/www-project-top-ten/", "Security"),
        new("soc-analyst-foundations", "MITRE ATT&CK", "https://attack.mitre.org/", "Security"),
        new("web-application-security", "OWASP Web Security Testing Guide", "https://owasp.org/www-project-web-security-testing-guide/", "Security"),
        new("sql-mysql-foundations", "MySQL 8.0 Reference Manual", "https://dev.mysql.com/doc/refman/8.0/en/", "Documentation"),
        new("certification-practice-python", "Python 3 Tutorial", "https://docs.python.org/3/tutorial/", "Exam Prep"),
        new("certification-practice-cloud", "AWS Cloud Practitioner Essentials", "https://aws.amazon.com/training/learn-about/cloud-practitioner/", "Exam Prep"),
        new("certification-practice-networking", "Cisco Learning Network", "https://learningnetwork.cisco.com/", "Exam Prep")
    ];
}
