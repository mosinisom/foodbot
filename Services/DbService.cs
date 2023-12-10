using Microsoft.EntityFrameworkCore;

public class DbService : IDisposable
{
    private readonly DataContext _context;
    public DbService(DataContext context)
    {
        _context = context;
    }


    public async Task AddUser(User user)
    {
        if (await _context.User.FirstOrDefaultAsync(u => u.chat_id == user.chat_id) != null)
            return;

        if (user.username == null)
            user.username = "no_username";
        
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task AddComponent(Component component)
    {
        await _context.Component.AddAsync(component);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUser(long chat_id)
    {
        return await _context.User.FirstOrDefaultAsync(u => u.chat_id == chat_id);
    }

    public async Task<Component?> GetComponent(int component_id)
    {
        return await _context.Component.FirstOrDefaultAsync(c => c.component_id == component_id);
    }

    public async Task<Component?> GetComponent(string component_name)
    {
        return await _context.Component.FirstOrDefaultAsync(c => c.name == component_name);
    }

    public async Task<List<Component>> GetAllComponents()
    {
        return await _context.Component.ToListAsync();
    }

    public async Task<Component?> GetOneRandomComponent()
    {
        Component? component = await _context.Component.FromSqlRaw("SELECT * FROM components ORDER BY RANDOM() LIMIT 1").FirstOrDefaultAsync();
        return component;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}