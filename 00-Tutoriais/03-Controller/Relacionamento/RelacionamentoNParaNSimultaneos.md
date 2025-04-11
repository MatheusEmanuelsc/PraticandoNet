# 🔄 ASP.NET Core 8 - Criação, Leitura e Atualização em Relacionamento N:N

## 📑 Índice

1. [Visão Geral](#visão-geral)
2. [Modelos de Entidade](#modelos-de-entidade)
3. [DTOs de Entrada e Leitura](#dtos-de-entrada-e-leitura)
4. [Configuração do AutoMapper](#configuração-do-automapper)
5. [Criação Simultânea (1x1 e NxN)](#criação-simultânea-1x1-e-nxn)
6. [Leitura de Relacionamentos](#leitura-de-relacionamentos)
7. [Atualização de Relacionamentos](#atualização-de-relacionamentos)
8. [Atualização Parcial (PATCH)](#atualização-parcial-patch)
9. [Remoção de Relacionamento](#remoção-de-relacionamento)
10. [Associação por ID de Entidades Existentes](#associação-por-id-de-entidades-existentes)
11. [Exemplo de JSON](#exemplo-de-json)
12. [Dicas Adicionais](#dicas-adicionais)

---

## 🔍 Visão Geral

Este guia mostra como criar, obter e atualizar relacionamentos **N:N** (muitos para muitos) usando ASP.NET Core 8. Utilizamos:

- Entidade de junção explícita (`StudentCourse`)
- AutoMapper
- DTOs de entrada e leitura
- Criação em lote e manipulação completa do relacionamento

---

## 🧱 Modelos de Entidade

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}

public class StudentCourse
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateTime EnrolledAt { get; set; }
}
```

---

## 📥 DTOs de Entrada e Leitura

### Entrada

```csharp
public class CourseCreateDto
{
    public string Title { get; set; } = string.Empty;
}

public class StudentCreateWithCoursesDto
{
    public string Name { get; set; } = string.Empty;
    public List<CourseCreateDto> Courses { get; set; } = new();
}

public class BatchStudentCreateDto
{
    public List<StudentCreateWithCoursesDto> Students { get; set; } = new();
}

public class CourseIdDto
{
    public int CourseId { get; set; }
}
```

### Leitura

```csharp
public class CourseReadDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}

public class StudentReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CourseReadDto> Courses { get; set; } = new();
}
```

---

## 🧭 Configuração do AutoMapper

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CourseCreateDto, Course>();
        CreateMap<StudentCreateWithCoursesDto, Student>();

        CreateMap<StudentCourse, CourseReadDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course.Title));

        CreateMap<Student, StudentReadDto>()
            .ForMember(dest => dest.Courses, opt =>
                opt.MapFrom(src => src.StudentCourses));
    }
}
```

---

## 🧪 Criação Simultânea (1x1 e NxN)

### Criar 1 estudante com seus cursos

```csharp
[HttpPost("students/create-with-courses")]
public async Task<IActionResult> Create([FromBody] StudentCreateWithCoursesDto dto)
{
    var student = new Student { Name = dto.Name };

    foreach (var courseDto in dto.Courses)
    {
        var course = _mapper.Map<Course>(courseDto);

        student.StudentCourses.Add(new StudentCourse
        {
            Student = student,
            Course = course,
            EnrolledAt = DateTime.UtcNow
        });
    }

    _context.Students.Add(student);
    await _context.SaveChangesAsync();

    return Ok();
}
```

### Criar vários estudantes com seus cursos

```csharp
[HttpPost("students/batch-create")]
public async Task<IActionResult> CreateMany([FromBody] BatchStudentCreateDto dto)
{
    foreach (var studentDto in dto.Students)
    {
        var student = new Student { Name = studentDto.Name };

        foreach (var courseDto in studentDto.Courses)
        {
            var course = _mapper.Map<Course>(courseDto);

            student.StudentCourses.Add(new StudentCourse
            {
                Student = student,
                Course = course,
                EnrolledAt = DateTime.UtcNow
            });
        }

        _context.Students.Add(student);
    }

    await _context.SaveChangesAsync();
    return Ok();
}
```

---

## 📖 Leitura de Relacionamentos

```csharp
[HttpGet("students/{id:int}")]
public async Task<ActionResult<StudentReadDto>> Get(int id)
{
    var student = await _context.Students
        .Include(s => s.StudentCourses)
        .ThenInclude(sc => sc.Course)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null) return NotFound();

    return Ok(_mapper.Map<StudentReadDto>(student));
}
```

---

## ♻️ Atualização de Relacionamentos (Substituir cursos)

```csharp
[HttpPut("students/{id:int}/courses")]
public async Task<IActionResult> UpdateCourses(int id, [FromBody] List<CourseCreateDto> newCourses)
{
    var student = await _context.Students
        .Include(s => s.StudentCourses)
        .ThenInclude(sc => sc.Course)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null) return NotFound();

    student.StudentCourses.Clear();

    foreach (var courseDto in newCourses)
    {
        var course = _mapper.Map<Course>(courseDto);
        student.StudentCourses.Add(new StudentCourse
        {
            StudentId = id,
            Course = course,
            EnrolledAt = DateTime.UtcNow
        });
    }

    await _context.SaveChangesAsync();
    return NoContent();
}
```

---

## 🔧 Atualização Parcial (PATCH)

```csharp
[HttpPatch("students/{id:int}/add-course")]
public async Task<IActionResult> AddCourse(int id, [FromBody] CourseCreateDto courseDto)
{
    var student = await _context.Students.FindAsync(id);
    if (student == null) return NotFound();

    var course = _mapper.Map<Course>(courseDto);

    _context.StudentCourses.Add(new StudentCourse
    {
        StudentId = id,
        Course = course,
        EnrolledAt = DateTime.UtcNow
    });

    await _context.SaveChangesAsync();
    return NoContent();
}
```

---

## ❌ Remoção de Relacionamento

```csharp
[HttpDelete("students/{studentId:int}/courses/{courseId:int}")]
public async Task<IActionResult> RemoveCourse(int studentId, int courseId)
{
    var relation = await _context.StudentCourses
        .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

    if (relation == null) return NotFound();

    _context.StudentCourses.Remove(relation);
    await _context.SaveChangesAsync();
    return NoContent();
}
```

---

## 🔗 Associação por ID de Entidades Existentes

```csharp
[HttpPost("students/{id:int}/add-existing-courses")]
public async Task<IActionResult> AddExistingCourses(int id, [FromBody] List<CourseIdDto> courseIds)
{
    var student = await _context.Students.FindAsync(id);
    if (student == null) return NotFound();

    foreach (var dto in courseIds)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
        if (!courseExists) continue;

        _context.StudentCourses.Add(new StudentCourse
        {
            StudentId = id,
            CourseId = dto.CourseId,
            EnrolledAt = DateTime.UtcNow
        });
    }

    await _context.SaveChangesAsync();
    return NoContent();
}
```

---

## 📦 Exemplo de JSON

```json
{
  "students": [
    {
      "name": "Carlos Souza",
      "courses": [
        { "title": "Biologia" },
        { "title": "Física I" }
      ]
    },
    {
      "name": "Fernanda Lima",
      "courses": [
        { "title": "Literatura" }
      ]
    }
  ]
}
```

---

## 💡 Dicas Adicionais

- Valide duplicidade de cursos (pelo título) antes de persistir.
- Pode-se usar IDs para associar cursos existentes.
- Substituir relacionamentos (PUT), adicionar (PATCH), remover (DELETE).
- FluentValidation pode ser aplicado nos DTOs para validar nomes, tamanhos, listas, etc.

---



