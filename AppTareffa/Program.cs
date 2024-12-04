using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//resgistrar o contexto como serviço no container
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TareffaDB"));




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello cat");

//aqui vem uma app de fora: 
//app.MapGet("Frase", async () => await new HttpClient().GetStringAsync(""));
app.MapGet("Frase", async () => await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

//agora o mapeamento do endpoints
app.MapGet("/tarefas", async (AppDbContext db) =>
{
    return await db.Tareffas.ToListAsync();
});


// retornar o endpoint pelo o ID
app.MapGet("/tareffa/{id}", async (int id, AppDbContext db) =>
    await db.Tareffas.FindAsync(id) is Tareffa tareffa ? Results.Ok(tareffa) : Results.NotFound()); 


//agor o endpoint com as tareffas que foram concluidas
app.MapGet("/tareffa/concluida", async (AppDbContext db) => await db.Tareffas.Where(t => t.Concluida).ToListAsync());


app.MapPost("/tarefa", async (Tareffa tareffa, AppDbContext db) =>
{
    db.Tareffas.Add(tareffa);
    await db.SaveChangesAsync();
    return Results.Created($"/{tareffa.Id}", tareffa);
});

//Agora atualizar
app.MapPut("/tareffas/{id}", async (int id, Tareffa inputTareffa, AppDbContext db) =>
{
    //aqui localizar a tareffa
    var tareffa = await db.Tareffas.FindAsync(id);

    if (tareffa is null) return Results.NotFound(); //aqui verifico se foi encontrada 

    tareffa.Nome = inputTareffa.Nome;  //atualizo os dados 
    tareffa.Concluida = inputTareffa.Concluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});



//apagar
app.MapDelete("/tareffa/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tareffas.FindAsync(id) is Tareffa tareffa)
    {
        db.Tareffas.Remove(tareffa);
        await db.SaveChangesAsync();
        return Results.Ok(tareffa);
    }

    return Results.NoContent();
});

app.Run();

//########################################

class Tareffa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool Concluida { get; set; }

}

//class de contexto, que defini as tarefas que será realizado. Faz o mapemento dos objetos. 
class AppDbContext : DbContext 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Tareffa> Tareffas => Set<Tareffa>();
}



