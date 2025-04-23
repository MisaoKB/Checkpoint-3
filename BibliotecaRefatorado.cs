using System;
using System.Collections.Generic;

namespace BibliotecaApp.Refatorado
{
    // Entidades
    public class Book
    {
        public string Title { get; }
        public string Author { get; }
        public string ISBN { get; }
        public bool IsAvailable { get; private set; } = true;

        public Book(string title, string author, string isbn)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
        }

        public void Borrow() => IsAvailable = false;
        public void Return() => IsAvailable = true;
    }

    public class User
    {
        public string Name { get; }
        public int Id { get; }

        public User(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }

    public class Loan
    {
        public Book Book { get; }
        public User User { get; }
        public DateTime LoanDate { get; }
        public DateTime DueDate { get; }
        public DateTime? ReturnDate { get; private set; }

        private const double FinePerDay = 1.0;

        public Loan(Book book, User user, int days)
        {
            Book = book;
            User = user;
            LoanDate = DateTime.Now;
            DueDate = LoanDate.AddDays(days);
        }

        public void MarkReturned()
        {
            ReturnDate = DateTime.Now;
            Book.Return();
        }

        public double CalculateFine()
        {
            if (!ReturnDate.HasValue || ReturnDate.Value <= DueDate)
                return 0;

            var daysLate = (ReturnDate.Value - DueDate).Days;
            return daysLate * FinePerDay;
        }
    }

    // Interfaces (abstrações)
    public interface IRepository<T>
    {
        void Add(T item);
        T Get(Func<T, bool> predicate);
        IEnumerable<T> GetAll();
    }

    public interface INotificationService
    {
        void Notify(User user, string subject, string message);
    }

    // Implementações de repositórios
    public class InMemoryRepository<T> : IRepository<T>
    {
        private readonly List<T> _items = new List<T>();
        public void Add(T item) => _items.Add(item);
        public T Get(Func<T, bool> predicate) => _items.Find(new Predicate<T>(predicate));
        public IEnumerable<T> GetAll() => _items;
    }

    // Serviço de notificação único para e-mail e SMS
    public class NotificationService : INotificationService
    {
        public void Notify(User user, string subject, string message)
        {
            // Aqui poderíamos injetar diferentes provedores (e-mail, SMS, push)
            Console.WriteLine($"[NOTIFY] Para: {user.Name} | {subject} - {message}");
        }
    }

    // Serviço principal da biblioteca
    public class LibraryService
    {
        private readonly IRepository<Book> _bookRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Loan> _loanRepo;
        private readonly INotificationService _notifier;

        public LibraryService(
            IRepository<Book> bookRepo,
            IRepository<User> userRepo,
            IRepository<Loan> loanRepo,
            INotificationService notifier)
        {
            _bookRepo = bookRepo;
            _userRepo = userRepo;
            _loanRepo = loanRepo;
            _notifier = notifier;
        }

        public void RegisterBook(string title, string author, string isbn)
        {
            var book = new Book(title, author, isbn);
            _bookRepo.Add(book);
            _notifier.Notify(null, "Novo Livro", $"{title} cadastrado na biblioteca.");
        }

        public void RegisterUser(string name, int id)
        {
            var user = new User(name, id);
            _userRepo.Add(user);
            _notifier.Notify(user, "Bem-vindo", "Você foi cadastrado na biblioteca.");
        }

        public bool BorrowBook(int userId, string isbn, int days)
        {
            var user = _userRepo.Get(u => u.Id == userId);
            var book = _bookRepo.Get(b => b.ISBN == isbn && b.IsAvailable);
            if (user == null || book == null) return false;

            book.Borrow();
            var loan = new Loan(book, user, days);
            _loanRepo.Add(loan);
            _notifier.Notify(user, "Empréstimo", $"Você pegou: {book.Title}");
            return true;
        }

        public double ReturnBook(int userId, string isbn)
        {
            var loan = _loanRepo.Get(l => l.User.Id == userId && l.Book.ISBN == isbn && !l.ReturnDate.HasValue);
            if (loan == null) return -1;

            loan.MarkReturned();
            var fine = loan.CalculateFine();
            if (fine > 0)
                _notifier.Notify(loan.User, "Multa", $"Você tem multa de R$ {fine}");
            return fine;
        }

        public IEnumerable<Book> GetAllBooks() => _bookRepo.GetAll();
        public IEnumerable<User> GetAllUsers() => _userRepo.GetAll();
        public IEnumerable<Loan> GetAllLoans() => _loanRepo.GetAll();
    }

    // Exemplo de uso
    class Program
    {
        static void Main()
        {
            var bookRepo = new InMemoryRepository<Book>();
            var userRepo = new InMemoryRepository<User>();
            var loanRepo = new InMemoryRepository<Loan>();
            var notifier = new NotificationService();

            var service = new LibraryService(bookRepo, userRepo, loanRepo, notifier);
            service.RegisterBook("Clean Code", "Robert C. Martin", "978-0132350884");
            service.RegisterUser("João Silva", 1);
            service.BorrowBook(1, "978-0132350884", 7);
            var fine = service.ReturnBook(1, "978-0132350884");
            Console.WriteLine($"Multa gerada: R$ {fine}");
        }
    }