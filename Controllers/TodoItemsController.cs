using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _db;

        public TodoItemsController(TodoContext db)
        {
            _db = db;
        }

        private bool TodoItemExists(long id)
        {
            return _db.TodoItems.Any(e => e.Id == id);
        }

        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete
            };

        //* POST: api/TodoItems
        //? To protect from over posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> PostTodoItem(TodoItemDTO todoDTO)
        {
            var todoItem = new TodoItem
            {
                IsComplete = todoDTO.IsComplete,
                Name = todoDTO.Name
            };

            _db.TodoItems.Add(todoItem);
            await _db.SaveChangesAsync();

            // return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem); // ใช้กับ TodoItem Model. ไม่ได้ผ่าน DTO
            return CreatedAtAction(nameof(PostTodoItem), new { id = todoItem.Id }, todoItem);
        }

        //* GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            return await _db.TodoItems.Where(q => q.IsDelete != true)
                                           .Select(x => ItemToDTO(x))
                                           .ToListAsync();
        }

        //* GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
        {
            var todoItem = await _db.TodoItems.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefaultAsync();

            if (todoItem == null)
            {
                return NotFound();
            }

            return ItemToDTO(todoItem);
        }

        //* PUT: api/TodoItems/5
        //? To protect from over posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItemDTO todoDTO)
        {
            if (id != todoDTO.Id)
            {
                return BadRequest();
            }

            // _db.Entry(todoItem).State = EntityState.Modified; // ใช้กับ TodoItem Model. ไม่ได้ผ่าน DTO

            var todoItem = await _db.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Name = todoDTO.Name;
            todoItem.IsComplete = todoDTO.IsComplete;
            todoItem.IsDelete = todoDTO.IsDelete;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content (Success Status Response)
        }

        //* DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var todoItem = await _db.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _db.TodoItems.Remove(todoItem); //! กรณีต้องการ ลบ ออกจากฐานข้อมูล
            await _db.SaveChangesAsync();

            return NoContent(); // 204 No Content (Success Status Response)
        }

        //* DELETE: api/TodoItems/soft-delete/5
        [HttpDelete("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteTodoItem(long id)
        {
            var todoItem = await _db.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.IsDelete = true;
            _db.Entry(todoItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent(); // 204 No Content (Success Status Response)
        }
    }
}
