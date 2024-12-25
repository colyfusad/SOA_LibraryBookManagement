using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerManagementService.Data;
using CustomerManagementService.Models;
using CustomerManagementService.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CustomerManagementService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public CustomersController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(new Response
            {
                Status = "Success",
                Message = "Customers retrieved successfully",
                Data = FormatCustomerResponse(customers)
            });
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(b => b.Id == id);

            if (customer == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = "NotFound!"
                });
            }

            return Ok(new Response 
            { 
                Status = "Fail",
                Message = "Get customer with id successfully!",
                Data = FormatCustomerResponse(customer) 
            });
        }

        // GET: api/Customers/by-cccd/{cccd}
        [HttpGet("by-cccd/{cccd}")]
        public async Task<ActionResult> GetCustomerByCCCD(string cccd)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CCCD == cccd);

            if (customer == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = "Customer not found!"
                });
            }

            return Ok(new Response
            {
                Status = "Success",
                Message = "Get customer by CCCD successfully!",
                Data = FormatCustomerResponse(customer)
            });
        }

        // PUT: api/Customers/id
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, [FromBody] CustomerDto customerDto)
        {
            var isExistInformation = _context.Customers.Any(e => e.Id != id && (e.CCCD == customerDto.CCCD || e.StudentId == customerDto.StudentId));
            if (isExistInformation)
            {
                return BadRequest(new Response
                {
                    Status = "Fail",
                    Message = "CCCD or StudentId is already in the system!"
                });
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = "IdCustomer not found"
                });
            } 

            customer.CCCD = customerDto.CCCD;
            customer.FullName = customerDto.FullName;
            customer.StudentId = customerDto.StudentId;
            customer.PhoneNumber = customerDto.PhoneNumber;
            customer.Email = customerDto.Email;
            customer.UpdateAt = DateTime.Now;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Customer updated successfully",
                    Data = FormatCustomerResponse(customer)
                });
            }

            return StatusCode(500, new Response
            {
                Status = "Fail",
                Message = "Failed to update customer"
            });
        }

        // POST: api/Customers
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer([FromBody] CustomerDto customerDto)
        {
            var isExistInformation = _context.Customers.Any(e => e.CCCD == customerDto.CCCD || e.StudentId == customerDto.StudentId);
            if (isExistInformation)
            {
                return BadRequest(new Response
                {
                    Status = "Fail",
                    Message = "CCCD or StudentId is already in the system!"
                });
            }
            
            var customer = new Customer
            {
                CCCD = customerDto.CCCD,
                FullName = customerDto.FullName,
                StudentId = customerDto.StudentId,
                PhoneNumber = customerDto.PhoneNumber,
                Email = customerDto.Email,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };
            _context.Customers.Add(customer);
            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Book is created!",
                    Data = FormatCustomerResponse(customer)
                });
            }

            return StatusCode(500, new Response
            {
                Status = "Fail",
                Message = "Failed to created customer"
            });
        }

        // DELETE: api/Customers/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = "IdCustomer not found"
                });
            }

            _context.Customers.Remove(customer);
            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Customer deleted successfully",
                });
            }

            return StatusCode(500, new Response
            {
                Status = "Fail",
                Message = "Failed to delete customer"
            });
        }

        private object FormatCustomerResponse(object input)
        {
            if (input is Customer customer)
            {
                return new
                {
                    customer.Id,
                    customer.CCCD,
                    customer.FullName,
                    customer.StudentId,
                    customer.PhoneNumber,
                    customer.Email,
                    CreatAt = customer.CreateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    UpdateAt = customer.UpdateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                };
            }
            else if (input is IEnumerable<Customer> customers)
            {
                return customers.Select(customer => new
                {
                    customer.Id,
                    customer.CCCD,
                    customer.FullName,
                    customer.StudentId,
                    customer.PhoneNumber,
                    customer.Email,
                    CreatAt = customer.CreateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    UpdateAt = customer.UpdateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                }).ToList();
            }

            throw new ArgumentException("Input must be of type Customer or IEnumerable<Customer>");
        }

    }
}
