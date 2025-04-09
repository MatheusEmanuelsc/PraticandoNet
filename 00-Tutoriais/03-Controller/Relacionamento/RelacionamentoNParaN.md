# 📘 ASP.NET Core 8 - Relacionamento N:N com Recurso Dependente

## 📑 Índice

1. [Visão Geral](#visão-geral)
2. [Modelos (Entities)](#modelos-entities)
3. [Configuração do DbContext](#configuração-do-dbcontext)
4. [DTOs e AutoMapper](#dtos-e-automapper)
5. [Repository e Unit of Work](#repository-e-unit-of-work)
6. [Controllers com Recurso Dependente](#controllers-com-recurso-dependente)
7. [PATCH com DTO e Validação](#patch-com-dto-e-validação)
8. [Conclusão](#conclusão)

---

## ✨ Visão Geral

Neste guia, implementamos um relacionamento **N:N (muitos para muitos)** no ASP.NET Core 8, utilizando entidade de junção explícita. Focamos no acesso via recurso dependente e nas boas práticas com `DTO`, `AutoMapper`, `Repository` e `UnitOfWork`.

> Exemplo: Um `Student` pode se matricular em muitos `Courses`, e um `Course` pode ter muitos `Students`.

---

## 🧱 Modelos (Entities)

### `Student.cs`
```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
```

### `Course.cs`
```csharp
public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
```

### `StudentCourse.cs` (entidade de junção)
```csharp
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

## 🛠️ Configuração do DbContext

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<StudentCourse>()
        .HasKey(sc => new { sc.StudentId, sc.CourseId });

    modelBuilder.Entity<StudentCourse>()
        .HasOne(sc => sc.Student)
        .WithMany(s => s.StudentCourses)
        .HasForeignKey(sc => sc.StudentId);

    modelBuilder.Entity<StudentCourse>()
        .HasOne(sc => sc.Course)
        .WithMany(c => c.StudentCourses)
        .HasForeignKey(sc => sc.CourseId);
}
```

> ✅ Essa configuração é **necessária** para N:N com entidade de junção explícita.

---

## 📜 DTOs e AutoMapper

### `EnrollStudentDto.cs`
```csharp
public class EnrollStudentDto
{
    public int CourseId { get; set; }
}
```

### `EnrollmentReadDto.cs`
```csharp
public class EnrollmentReadDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}
```

### `MappingProfile.cs`
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<StudentCourse, EnrollmentReadDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title));
    }
}
```

---

## 📦 Repository e Unit of Work

### `IStudentCourseRepository.cs`
```csharp
public interface IStudentCourseRepository
{
    Task<IEnumerable<StudentCourse>> GetCoursesByStudentAsync(int studentId);
    Task<StudentCourse?> GetAsync(int studentId, int courseId);
    Task AddAsync(StudentCourse entity);
    void Remove(StudentCourse entity);
}
```

> Este repositório trata das operações da entidade de junção.

---

## 🎮 Controllers com Recurso Dependente

```csharp
[ApiController]
[Route("api/students/{studentId:int}/courses")]
public class StudentCoursesController : ControllerBase
{
    private readonly IStudentCourseRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StudentCoursesController(IStudentCourseRepository repo, IUnitOfWork uow, IMapper mapper)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
    }

    // GET: /api/students/1/courses
    [HttpGet]
    public async Task<IActionResult> GetAll(int studentId)
    {
        var list = await _repo.GetCoursesByStudentAsync(studentId);
        return Ok(_mapper.Map<IEnumerable<EnrollmentReadDto>>(list));
    }

    // POST: /api/students/1/courses
    [HttpPost]
    public async Task<IActionResult> Enroll(int studentId, [FromBody] EnrollStudentDto dto)
    {
        var exists = await _repo.GetAsync(studentId, dto.CourseId);
        if (exists is not null) return Conflict("Student already enrolled in this course.");

        var enrollment = new StudentCourse
        {
            StudentId = studentId,
            CourseId = dto.CourseId,
            EnrolledAt = DateTime.UtcNow
        };

        await _repo.AddAsync(enrollment);
        await _uow.CommitAsync();

        return CreatedAtAction(nameof(GetAll), new { studentId }, null);
    }

    // DELETE: /api/students/1/courses/5
    [HttpDelete("{courseId:int}")]
    public async Task<IActionResult> Unenroll(int studentId, int courseId)
    {
        var enrollment = await _repo.GetAsync(studentId, courseId);
        if (enrollment is null) return NotFound();

        _repo.Remove(enrollment);
        await _uow.CommitAsync();
        return NoContent();
    }
}
```

> ✅ Cada rota está aninhada ao `Student`. A entidade de junção é manipulada diretamente.

---

## 🔁 PATCH com DTO e Validação

### DTO Parcial
```csharp
public class PatchEnrollmentDto
{
    public DateTime? EnrolledAt { get; set; }
}
```

### Validação com FluentValidation
```csharp
public class PatchEnrollmentDtoValidator : AbstractValidator<PatchEnrollmentDto>
{
    public PatchEnrollmentDtoValidator()
    {
        RuleFor(x => x.EnrolledAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.EnrolledAt.HasValue)
            .WithMessage("Enrollment date must be in the past or today.");
    }
}
```

### Rota PATCH no Controller
```csharp
[HttpPatch("{courseId:int}")]
public async Task<IActionResult> Patch(int studentId, int courseId, [FromBody] JsonPatchDocument<PatchEnrollmentDto> patchDoc)
{
    if (patchDoc is null) return BadRequest();

    var enrollment = await _repo.GetAsync(studentId, courseId);
    if (enrollment is null) return NotFound();

    var dto = new PatchEnrollmentDto
    {
        EnrolledAt = enrollment.EnrolledAt
    };

    patchDoc.ApplyTo(dto, ModelState);
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var validator = new PatchEnrollmentDtoValidator();
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        validationResult.AddToModelState(ModelState);
        return ValidationProblem(ModelState);
    }

    // Aplica alterações
    if (dto.EnrolledAt.HasValue)
        enrollment.EnrolledAt = dto.EnrolledAt.Value;

    await _uow.CommitAsync();
    return NoContent();
}
```

---

## ✅ Conclusão

- A relação N:N exige uma entidade de junção explícita para controle detalhado.
- É essencial configurar manualmente o relacionamento no `DbContext`.
- Rotas aninhadas ajudam a expressar a dependência do recurso.
- O repositório para a entidade de junção deve tratar dos casos de inscrição, leitura e remoção.
- `DTOs` e `AutoMapper` garantem separação de responsabilidades.
- `PATCH` pode ser implementado com segurança usando `JsonPatchDocument` + DTO + validação parcial com `FluentValidation`.



