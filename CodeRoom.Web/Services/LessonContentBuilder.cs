namespace CodeRoom.Web.Services;

public static class LessonContentBuilder
{
    public static string Build(string courseTitle, string lessonTitle, int lessonNumber) =>
        EnsureRichContent(null, courseTitle, lessonTitle, lessonNumber);

    public static string EnsureRichContent(string? existingContent, string courseTitle, string lessonTitle, int lessonNumber)
    {
        if (!string.IsNullOrWhiteSpace(existingContent) && existingContent.Trim().Length >= 260)
        {
            return existingContent.Trim();
        }

        var focus = GetFocus(courseTitle, lessonTitle);
        var practice = GetPractice(lessonTitle);

        var application = GetApplication(courseTitle, lessonTitle);
        var mistakes = GetCommonMistakes(lessonTitle);

        return
            $"Lesson {lessonNumber} — {lessonTitle}: {focus}\n\n" +
            $"WHY IT MATTERS: {courseTitle} relies on this concept because it helps you make predictable technical decisions instead of memorising isolated commands or definitions. " +
            $"The goal is to understand what the concept does, when it is useful, and what evidence you can use to verify your result.\n\n" +
            $"KEY CONCEPTS: Break the topic into three questions: what is it, how does it work, and what changes when the inputs change? " +
            $"Keep the terms precise, identify the important boundaries, and connect each rule to at least one concrete example. This makes the lesson easier to recall during troubleshooting and assessment questions.\n\n" +
            $"HOW TO THINK ABOUT IT: Start by identifying the inputs, the expected behaviour and the result you need. " +
            $"Then work through the process one step at a time. If the result differs from your expectation, compare the actual evidence with your assumption and isolate the smallest part that could explain the difference.\n\n" +
            $"WORKED EXAMPLE: {application} " +
            $"Write down the expected result before you run the example, then compare it with the actual output. If the result is different, change one variable at a time rather than changing the whole example at once.\n\n" +
            $"COMMON MISTAKES: {mistakes}\n\n" +
            $"PRACTICE FOCUS: {practice} Reproduce a small example yourself, change one part, and record what changed. " +
            $"For a stronger test, include one normal case and one edge case, then explain why the two results differ.\n\n" +
            $"CHECK YOURSELF: Can you define the concept, describe one realistic use case, predict the result before execution, and explain one failure mode? " +
            $"If not, revisit the key concepts before moving on.\n\n" +
            $"KEY TAKEAWAY: Write one sentence in your own words, one command or pattern you would use in practice, and one question you would ask during a real technical investigation or project.";
    }

    private static string GetApplication(string courseTitle, string lessonTitle) =>
        $"Create a small example centred on {lessonTitle}. First write down what you expect the example to do, then run it and compare the actual result. " +
        $"For {courseTitle}, try to relate the example to a realistic task such as building a feature, analysing input, troubleshooting a problem or protecting a system.";

    private static string GetCommonMistakes(string title) => title switch
    {
        "Variables & Data Types" => "Using a type without considering the kind of value it represents, converting data without checking the result, or changing values in more places than necessary.",
        "Loops" or "Conditional Statements" or "Conditions" => "Making the condition too broad, forgetting an exit condition, or assuming every iteration or branch will run.",
        "Methods" or "Functions" => "Making one method responsible for too many unrelated tasks, using unclear parameter names, or duplicating logic instead of reusing a well-defined function.",
        "Classes & Objects" => "Putting unrelated responsibilities into one class or exposing data without considering how the object should control its own state.",
        "Inheritance & Polymorphism" => "Using inheritance simply for code reuse when composition or a shared interface would communicate the design more clearly.",
        "HTML Structure" or "Semantic HTML" => "Choosing elements based only on visual appearance instead of the meaning of the content, or skipping labels and document structure that users and assistive technology rely on.",
        "CSS Fundamentals" or "Box Model" or "Flexbox" or "Grid" => "Changing several layout properties at once without checking which property is actually controlling the result.",
        "Responsive Design" => "Designing only for one screen width and trying to patch smaller screens with lots of fixed dimensions.",
        "Forms & Accessibility" or "Forms & Validation" => "Relying only on browser-side checks and assuming submitted data is trustworthy.",
        "Routing" => "Creating links that depend on hard-coded URLs or changing a route without checking the navigation paths that depend on it.",
        "Entity Framework Core" or "Database Design" => "Ignoring relationships and constraints because the application appears to work with a small sample of data.",
        "Authentication" or "Authentication & Authorization" => "Confusing identity verification with permission checks or granting broader access than the user actually needs.",
        "OSI Model" or "TCP/IP Model" => "Treating the models as seven or four exact physical boxes rather than conceptual ways of organising networking responsibilities.",
        "TCP & UDP" => "Choosing a transport protocol from habit without considering reliability, latency and application behaviour.",
        "DNS" => "Assuming every name lookup is a simple one-to-one mapping and overlooking caching, record types and the possibility of failures at different points.",
        "HTTP/HTTPS" => "Looking only at the page content instead of checking the request method, status code, headers and transport security.",
        "Permissions" => "Granting broad access because it makes a test work instead of applying the least privilege required.",
        "Security Monitoring" => "Treating a single event as proof of compromise without checking surrounding context, timing, source and related activity.",
        "Phishing" => "Trusting urgency, display names or familiar branding without independently verifying the sender and destination.",
        "Normalization" => "Removing duplication mechanically without first understanding the relationships and access patterns the application needs.",
        _ => "Skipping the small verification step and assuming the first result proves that the concept is understood."
    };

    private static string GetFocus(string courseTitle, string title) => title switch
    {
        "Introduction to C#" => "C# is a statically typed, object-oriented language used on the .NET platform. This lesson establishes the role of source files, namespaces, statements, methods and the program entry point.",
        "Variables & Data Types" => "Variables hold values that a program can work with, while data types describe the kind of value being stored. In C#, choosing an appropriate type helps the compiler catch mistakes early and makes the program's intent clearer.",
        "Operators & Expressions" => "Operators let a program perform arithmetic, comparisons and logical decisions. Expressions combine values and operators into results that can be assigned, checked or passed into methods.",
        "Conditional Statements" => "Conditional statements allow a program to choose different actions based on a condition. The common building blocks are if, else if, else and switch, each useful for a different style of decision logic.",
        "Loops" => "Loops repeat a block of work while a condition or sequence requires it. for, while, do-while and foreach loops are useful in different situations, especially when processing collections or repeating a known operation.",
        "Methods" => "Methods package reusable behaviour behind a clear name, parameters and return type. Breaking a larger task into methods makes code easier to test, understand and maintain.",
        "Arrays & Collections" => "Collections organise multiple values so a program can process related data together. Arrays provide a fixed-size structure, while collection types such as List and Dictionary are more flexible for everyday applications.",
        "Classes & Objects" => "Classes define the structure and behaviour of objects. This is the foundation of object-oriented programming in C#, where data and the operations that work on that data can be modelled together.",
        "Inheritance & Polymorphism" => "Inheritance lets a derived class reuse members from a base class, while polymorphism allows code to work with a shared abstraction and still use the specialised behaviour of a derived type.",
        "Python Basics" => "Python emphasises readable syntax and a small amount of ceremony around common programming tasks. This lesson introduces the interpreter, scripts, indentation and the basic shape of a Python program.",
        "Conditions" => "Python uses if, elif and else to branch execution. Conditions are expressions that evaluate to true or false and can be combined to create more precise decision rules.",
        "Functions" => "Functions group reusable behaviour and can accept parameters and return values. Good functions reduce repetition and give a program a clearer structure.",
        "Lists, Tuples & Dictionaries" => "Python provides several built-in collection types for different needs. Lists are mutable sequences, tuples are immutable sequences, and dictionaries store key-value pairs for direct lookup.",
        "Modules" => "Modules let Python code be split into reusable files and imported where needed. Working with modules makes larger programs easier to organise and helps keep individual files focused.",
        "File Handling" => "File handling allows programs to read information from and write information to persistent storage. The with statement is commonly used so files are closed correctly even when an operation fails.",
        "Exceptions" => "Exceptions represent problems that occur while a program is running. try, except, else and finally provide a structured way to handle expected failures without hiding the underlying problem.",
        "How the Web Works" => "Web applications rely on a client-server model in which a browser sends HTTP requests and a web server returns responses. Understanding URLs, requests, responses and status codes provides the foundation for everything built later in the course.",
        "HTML Structure" => "HTML describes the structure and meaning of a web page. Elements, attributes, headings, paragraphs, lists, links and media combine to create a document that browsers and assistive technologies can understand.",
        "Semantic HTML" => "Semantic elements communicate the role of page content, such as navigation, main content, articles and footers. Clear semantics improve maintainability, accessibility and the way content is interpreted by tools.",
        "CSS Fundamentals" => "CSS controls how HTML content is presented. Selectors, properties and values work together to manage typography, spacing, colour, borders and layout without mixing presentation into the document structure.",
        "Box Model" => "Every normal HTML element can be understood through the CSS box model: content, padding, border and margin. Knowing how those layers affect size and spacing makes layout problems much easier to diagnose.",
        "Flexbox" => "Flexbox is designed for arranging items along one main axis while giving control over alignment, spacing and distribution. It is especially useful for navigation bars, toolbars and component rows.",
        "Grid" => "CSS Grid provides a two-dimensional layout system for rows and columns. It is useful for page sections, dashboards and card layouts where both horizontal and vertical relationships matter.",
        "Responsive Design" => "Responsive design adapts a page to different screen sizes and input conditions. Flexible units, media queries and responsive layouts allow the same interface to remain usable on phones, tablets and desktops.",
        "Forms & Accessibility" => "Forms are the main mechanism for collecting user input on the web. Good labels, meaningful controls, validation messages, keyboard support and sensible focus order make forms easier for everyone to use.",
        "Introduction to ASP.NET Core" => "ASP.NET Core is a cross-platform web framework for building server-side applications on .NET. This lesson introduces the application lifecycle, dependency injection, middleware and the role of the host.",
        "MVC Architecture" => "The MVC pattern separates request handling, application data and presentation into controllers, models and views. This separation makes a web application easier to reason about and change as it grows.",
        "Controllers" => "Controllers receive HTTP requests, coordinate application work and return an appropriate response. In MVC applications, action methods usually fetch or update data and then choose a view or redirect.",
        "Models" => "Models represent the application's data and the rules that apply to it. Data annotations can describe validation requirements, display names and database-facing constraints.",
        "Razor Views" => "Razor combines HTML with C# expressions so server-side data can be rendered into a page. Keeping views focused on presentation helps avoid putting business logic in the UI layer.",
        "Routing" => "Routing maps an incoming URL to a controller action or endpoint. Clean routes make an application easier to navigate and allow links to remain meaningful as the system grows.",
        "Forms & Validation" => "Server-side validation ensures that data cannot be trusted simply because it came from a browser. ASP.NET Core validation works alongside client-side validation to give users fast feedback while keeping the server authoritative.",
        "Entity Framework Core" => "Entity Framework Core provides object-relational mapping between C# classes and relational database tables. DbContext, DbSet and LINQ queries let the application read and update MySQL data through strongly typed code.",
        "Authentication" => "Authentication establishes who the current user is, while authorization determines what that user is allowed to do. Cookie authentication and role checks provide the foundation for protected student and administrator areas.",
        "Building an MVC Application" => "A complete MVC application brings together models, views, controllers, routing, validation, dependency injection and persistence. The aim is to connect these pieces into a maintainable end-to-end workflow.",
        "What is a Network?" => "A computer network allows devices to communicate and share resources using agreed protocols. The lesson introduces endpoints, links, addressing and the basic purpose of switches, routers and other network components.",
        "OSI Model" => "The OSI model divides network communication into seven conceptual layers. It is a troubleshooting and learning framework that helps relate application behaviour to transport, network, data-link and physical processes.",
        "TCP/IP Model" => "The TCP/IP model groups networking functions into layers used by real Internet protocols. Understanding how application, transport, Internet and link functions interact makes packet captures easier to interpret.",
        "IPv4 & IPv6" => "IP addressing identifies interfaces so packets can be delivered across networks. IPv4 uses 32-bit addresses, while IPv6 expands the address space and introduces a different notation and feature set.",
        "MAC & ARP" => "MAC addresses identify interfaces at the local network level, while ARP resolves an IPv4 address to a local MAC address. Together they explain an important part of how an Ethernet host reaches a neighbour.",
        "TCP & UDP" => "TCP provides a connection-oriented transport with sequencing, acknowledgements and retransmission, while UDP provides a lightweight datagram service. Choosing between them depends on the application's delivery requirements.",
        "Ports & Protocols" => "Ports distinguish services on a host, while protocols define how those services communicate. Recognising common port and protocol combinations is fundamental to network troubleshooting and security monitoring.",
        "DNS" => "DNS translates human-readable names into records such as IP addresses. Clients use DNS so applications can refer to services by names instead of hard-coding addresses.",
        "HTTP/HTTPS" => "HTTP defines the request-response exchange used by the web, while HTTPS protects that exchange with TLS. Methods, headers, status codes and encrypted transport all matter when analysing web traffic.",
        "Network Troubleshooting" => "Troubleshooting follows a structured process: define the symptom, isolate the layer or component involved, test a hypothesis and verify the result. Tools such as ping, traceroute, ipconfig or Wireshark support that process with evidence.",
        "Linux Basics" => "Linux is a family of Unix-like operating systems built around the kernel and a collection of user-space tools. This lesson introduces distributions, shells and the basic workflow of working from a terminal.",
        "Filesystem" => "The Linux filesystem is organised as a hierarchy starting from the root directory. Important paths such as /etc, /var, /home and /usr have different purposes and help administrators locate configuration, logs, user data and software.",
        "Navigation & CLI" => "Command-line navigation uses a small set of tools such as pwd, ls and cd to locate and inspect files. Learning these commands first makes later administration and troubleshooting much faster.",
        "Users & Groups" => "Linux uses users and groups to control identity and access. Group membership provides a convenient way to assign permissions to a set of users without changing each account individually.",
        "Permissions" => "Linux permissions control read, write and execute access for an owner, a group and other users. Understanding chmod, chown and permission notation is essential for safe system administration.",
        "Processes" => "A process is a running instance of a program. Linux tools such as ps, top and kill help administrators inspect process activity and manage programs that are consuming resources or behaving unexpectedly.",
        "Networking Commands" => "Linux provides command-line tools for inspecting interfaces, routes, sockets and DNS. These utilities provide quick evidence when diagnosing connectivity or suspicious network activity.",
        "Package Management" => "Package managers install, update and remove software while handling dependencies. Learning the package workflow makes system maintenance more consistent than manually copying application files.",
        "Shell Basics" => "A shell interprets commands and provides scripting features such as variables, conditions, loops and pipelines. These capabilities make repetitive administration tasks easier to automate.",
        "Linux Security Basics" => "Basic Linux security combines least privilege, strong permissions, timely updates, secure services and useful logging. These controls reduce the chance that a compromised process can affect the entire system.",
        "Introduction to Cybersecurity" => "Cybersecurity protects information, systems and services from unauthorised access, misuse and disruption. This lesson introduces defensive thinking, risk and the relationship between people, processes and technology.",
        "CIA Triad" => "The confidentiality, integrity and availability triad provides a simple way to reason about security objectives. Different incidents affect these properties in different ways, so controls should be matched to the risk.",
        "Threats & Vulnerabilities" => "A threat is something capable of causing harm, while a vulnerability is a weakness that can be exploited. Security work connects the two through risk analysis and prioritised remediation.",
        "Authentication & Authorization" => "Authentication verifies identity, whereas authorization evaluates permissions after identity has been established. Separating these concerns helps systems enforce least privilege and safer access decisions.",
        "Malware" => "Malware is software designed to perform unwanted or harmful actions. Common categories include trojans, ransomware, spyware and worms, each with different behaviours and defensive indicators.",
        "Phishing" => "Phishing uses deceptive messages or interfaces to manipulate users into revealing information or taking unsafe actions. Defensive awareness focuses on sender identity, links, urgency cues and verification through trusted channels.",
        "Firewalls" => "Firewalls enforce traffic rules between networks or hosts. A useful rule set is explicit about sources, destinations, protocols and ports while following least privilege.",
        "Endpoint Security" => "Endpoint security combines controls such as secure configuration, patching, anti-malware, application control and monitoring. The goal is to reduce attack surface while generating useful evidence when something abnormal occurs.",
        "Security Monitoring" => "Security monitoring collects events and telemetry so defenders can recognise suspicious activity and investigate it. Good monitoring depends on useful log sources, timestamps, context and alert quality.",
        "Incident Response Basics" => "Incident response provides a repeatable way to prepare for, identify, contain, eradicate and recover from security incidents. Clear evidence and documented actions help teams reduce impact and learn from each event.",
        "Introduction to Databases" => "Databases provide structured storage and controlled access to information. This lesson introduces tables, records, relationships and the role of a database management system in an application.",
        "Relational Databases" => "Relational databases organise data into tables that can be linked through keys. The relational model supports consistency and powerful queries while keeping related data structured.",
        "Tables & Relationships" => "Tables store rows of related records, while relationships connect records across tables. One-to-many relationships are especially common in applications such as courses with many lessons.",
        "Primary & Foreign Keys" => "A primary key uniquely identifies a record, while a foreign key references a record in another table. Together they create reliable relationships and prevent ambiguous links between data.",
        "SQL SELECT" => "SELECT retrieves data from one or more tables. Filtering with WHERE, ordering with ORDER BY and combining rows with expressions lets an application retrieve exactly the information it needs.",
        "INSERT / UPDATE / DELETE" => "INSERT creates records, UPDATE changes existing records and DELETE removes them. These operations must be used carefully, especially when constraints or user-generated data are involved.",
        "JOINs" => "JOINs combine related rows from multiple tables based on a relationship. INNER JOIN, LEFT JOIN and other variants answer different questions about matching and missing records.",
        "Constraints" => "Constraints enforce data rules at the database level. Primary keys, unique constraints, foreign keys and required values help protect integrity even when multiple parts of an application write to the same database.",
        "Normalization" => "Normalization reduces unnecessary duplication by separating data into well-structured related tables. The aim is to improve consistency and make updates less error-prone.",
        "Database Design" => "Good database design starts from application requirements and turns them into entities, attributes and relationships. Thinking carefully about keys, constraints and expected queries makes the final schema easier to maintain.",
        _ => $"This topic explains an important part of {courseTitle}. Focus on the terminology, the purpose of the concept, the conditions in which it is used and the mistakes that commonly lead to incorrect results."
    };

    private static string GetPractice(string title) => title switch
    {
        "Variables & Data Types" or "SQL SELECT" => "Create a small example, change one value or condition, and predict the result before running it.",
        "Loops" or "Conditions" or "Conditional Statements" => "Write a small decision or repetition problem such as filtering a list of values or counting matching items.",
        "Classes & Objects" or "Models" => "Design a small class with two or three properties and one behaviour that uses those properties.",
        "Authentication" or "Authentication & Authorization" => "Trace a login request and identify where identity is established and where access permissions are checked.",
        "HTTP/HTTPS" => "Inspect a request in browser developer tools and identify the method, URL, status code and important headers.",
        "OSI Model" or "TCP/IP Model" => "Take a common web request and describe which networking layer or function is responsible for each stage.",
        "Permissions" => "Create a test file and inspect its permission bits, then explain which users should be allowed to read or modify it.",
        "Security Monitoring" => "Pick a security event and list the log fields you would need to determine who acted, what happened, when it happened and from where.",
        "Normalization" or "Database Design" => "Sketch the entities involved in a simple learning system and decide which relationships should be represented with foreign keys.",
        "JOINs" => "Write a query that combines two related tables and compare the result of INNER JOIN with LEFT JOIN.",
        "Mini Project" => "Combine the ideas from the preceding lessons into a small working exercise. Keep the scope narrow, test the happy path, then deliberately try an invalid input.",
        _ => "Create a tiny example that demonstrates the idea, test it with a normal case and one edge case, and write down what you learned from the result."
    };
}
