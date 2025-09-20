using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OrdersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class UserAccessesController : Controller
    {
        private readonly OzonOrderContext _context;
        private readonly List<string> AvailableOrderColumns = new List<string>
        {
            "Номер заказа",
            "Клиент",
            "Принят в обработку",
            "Дата отгрузки",
            "Срок доставки",
            "Статус клиента",
            "Статус",
            "Наименование товара",
            "Артикул",
            "Key",
            "Производитель",
            "Склад отгрузки",
            "Поставщик",
            "Номер заказа поставщику",
            "Цена сайта",
            "Цена",
            "Количество",
            "Сумма отправления",
            "Категория",
            "Объемный вес",
            "Цена закупки",
            "Комиссия ОЗОН",
            "Прибыль",
            "Наценка %",
            "Себестоимость",
            "Город доставки"
        };
        public UserAccessesController(OzonOrderContext context)
        {
            _context = context;
        }

        // GET: UserAccesses1
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserAccess.ToListAsync());
        }

        // GET: UserAccesses1/Create
        public IActionResult Create()
        {
            ViewData["AvailableOrderColumns"] = AvailableOrderColumns;
            return View();
        }


        // POST: UserAccesses1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAccess userAccess)
        {
            if (ModelState.IsValid)
            {
                var isUnique = _context.UserAccess.FirstOrDefault(ua => ua.Name == userAccess.Name) == null;
                if (isUnique && userAccess.AvailableOrderColumns != null)
                {
                    _context.Add(userAccess);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["AvailableOrderColumns"] = AvailableOrderColumns;
            return View(userAccess);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAccess = await _context.UserAccess.FindAsync(id);
            if (userAccess == null)
            {
                return NotFound();
            }

            ViewData["AvailableOrderColumns"] = AvailableOrderColumns;
            return View(userAccess);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AvailableOrderColumns")] UserAccess userAccess)
        {
            if (id != userAccess.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userAccess);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserAccessExists(userAccess.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AvailableOrderColumns"] = AvailableOrderColumns;
            return View(userAccess);
        }


        // GET: UserAccesses1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAccess = await _context.UserAccess
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userAccess == null)
            {
                return NotFound();
            }

            return View(userAccess);
        }

        // POST: UserAccesses1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userAccess = await _context.UserAccess.FindAsync(id);
            if (userAccess != null)
            {
                _context.UserAccess.Remove(userAccess);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserAccessExists(int id)
        {
            return _context.UserAccess.Any(e => e.Id == id);
        }
    }
}
