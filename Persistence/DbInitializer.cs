using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TruequeU.Enums;
using TruequeU.Models;

namespace TruequeU.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        if (await context.Users.AnyAsync().ConfigureAwait(false))
            return;

        var users = await SeedUsersAsync(userManager).ConfigureAwait(false);
        var adminUser = users[0];

        var listings = await SeedListingsAsync(context, users).ConfigureAwait(false);
        await SeedImagesAsync(context, listings).ConfigureAwait(false);
        await SeedConversationsAndMessagesAsync(context, users, listings).ConfigureAwait(false);
        await SeedReportsAsync(context, users, listings, adminUser).ConfigureAwait(false);
    }

    private static async Task<List<User>> SeedUsersAsync(UserManager<User> userManager)
    {
        var users = new List<User>();
        var userData = new[]
        {
            ("carlos.medina", "carlos.medina@eia.edu.co", "Carlos Medina", "Ingeniería de Sistemas", "Admin"),
            ("maria.garcia", "maria.garcia@eia.edu.co", "María García", "Ingeniería Industrial", "User"),
            ("juan.lopez", "juan.lopez@eia.edu.co", "Juan López", "Ingeniería Mecánica", "User"),
            ("ana.martinez", "ana.martinez@eia.edu.co", "Ana Martínez", "Ingeniería Civil", "User"),
            ("pedro.rodriguez", "pedro.rodriguez@eia.edu.co", "Pedro Rodríguez", "Administración de Empresas", "User"),
            ("laura.fernandez", "laura.fernandez@eia.edu.co", "Laura Fernández", "Diseño Industrial", "User"),
            ("diego.torres", "diego.torres@eia.edu.co", "Diego Torres", "Ingeniería Biomédica", "User"),
            ("sofia.ramirez", "sofia.ramirez@eia.edu.co", "Sofía Ramírez", "Economía", "User"),
            ("andres.castro", "andres.castro@eia.edu.co", "Andrés Castro", "Ingeniería Electrónica", "User"),
            ("valentina.diaz", "valentina.diaz@eia.edu.co", "Valentina Díaz", "Negocios Internacionales", "User"),
        };

        var ratings = new[] { 4.8, 4.5, 4.2, 4.7, 3.9, 4.1, 4.3, 4.6, 2.8, 4.0 };

        for (int i = 0; i < userData.Length; i++)
        {
            var (userName, email, fullName, program, role) = userData[i];
            var user = new User
            {
                UserName = userName,
                Email = email,
                FullName = fullName,
                Program = program,
                Rating = ratings[i],
                Bio = $"Estudiante de {program} en la Universidad EIA.",
                State = i == 8 ? UserState.Suspended : UserState.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-90 + i * 7),
                LastLogin = DateTime.UtcNow.AddDays(-i),
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Password123!").ConfigureAwait(false);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role).ConfigureAwait(false);

            users.Add(user);
        }

        return users;
    }

    private static async Task<List<Listing>> SeedListingsAsync(ApplicationDbContext context, List<User> users)
    {
        var listings = new List<Listing>();
        var seedData = new (string Title, string Description, decimal Price, Category Category, ItemCondition Condition, string Campus, ListingState State, int OwnerIndex)[]
        {
            ("Calculadora Científica Casio FX-991", "Calculadora científica en excelente estado, ideal para exámenes de ingeniería. Incluye estuche original.", 80000, Category.Electronics, ItemCondition.LikeNew, "Bloque 18", ListingState.Available, 1),
            ("Libro: Cálculo de Stewart 8va Edición", "Libro de cálculo en buen estado, con algunos apuntes a lápiz. Perfecto para primer semestre.", 120000, Category.Books, ItemCondition.UsedGood, "Biblioteca", ListingState.Available, 2),
            ("Monitor Dell 24 pulgadas", "Monitor Full HD 1080p, HDMI y VGA. Ideal para setup de estudio o trabajo remoto.", 350000, Category.Electronics, ItemCondition.UsedGood, "Bloque 20", ListingState.Available, 0),
            ("Escritorio de madera con cajones", "Escritorio amplio de madera color nogal. Tiene algunos rayones pero está en buen estado funcional.", 250000, Category.Furniture, ItemCondition.UsedFair, "Bloque 18", ListingState.Available, 3),
            ("Chaqueta universitaria EIA talla M", "Chaqueta oficial de la universidad, prácticamente nueva. Solo la usé una vez.", 90000, Category.Clothing, ItemCondition.LikeNew, "Bloque 19", ListingState.Available, 4),
            ("iPad Air 4ta Gen 64GB", "iPad en perfecto estado con Apple Pencil incluido. Ideal para tomar apuntes digitales.", 1800000, Category.Electronics, ItemCondition.LikeNew, "Bloque 20", ListingState.Reserved, 5),
            ("Bicicleta Trek Marlin 5", "Bicicleta de montaña en buen estado. Perfecta para moverte por el campus.", 750000, Category.Other, ItemCondition.UsedGood, "Parqueadero Bicicletas", ListingState.Available, 6),
            ("Audífonos Sony WH-1000XM4", "Audífonos con cancelación de ruido activa, ideales para estudiar en la biblioteca.", 450000, Category.Electronics, ItemCondition.LikeNew, "Biblioteca", ListingState.Available, 7),
            ("Libro: Mecánica de Fluidos - Cengel", "Libro de mecánica de fluidos, excelente estado. Incluye código de acceso online sin usar.", 150000, Category.Books, ItemCondition.LikeNew, "Bloque 18", ListingState.Available, 1),
            ("Silla ergonómica de oficina", "Silla ergonómica con soporte lumbar ajustable. Muy cómoda para largas sesiones de estudio.", 300000, Category.Furniture, ItemCondition.UsedGood, "Bloque 20", ListingState.Available, 2),
            ("Camiseta selección Colombia 2024", "Camiseta original de la selección Colombia, talla L. Nueva sin estrenar.", 180000, Category.Clothing, ItemCondition.New, "Bloque 19", ListingState.Available, 4),
            ("MacBook Pro 2021 M1 Pro", "MacBook Pro 14 pulgadas, 16GB RAM, 512GB SSD. Incluye cargador original y funda.", 6500000, Category.Electronics, ItemCondition.UsedGood, "Bloque 20", ListingState.Available, 0),
            ("Apuntes completos de Termodinámica", "Apuntes digitales completos de la materia de Termodinámica. Incluye ejercicios resueltos.", 35000, Category.Other, ItemCondition.New, "Virtual", ListingState.Available, 6),
            ("Mueble para microondas", "Pequeño mueble de cocina con espacio para microondas. Color blanco, buen estado.", 100000, Category.Furniture, ItemCondition.UsedGood, "Bloque 18", ListingState.Available, 5),
            ("Sudadera universitaria EIA talla S", "Sudadera oficial con el logo bordado. Color azul marino. Solo puesta un par de veces.", 75000, Category.Clothing, ItemCondition.LikeNew, "Bloque 19", ListingState.Sold, 3),
        };

        foreach (var (title, description, price, category, condition, campus, state, ownerIndex) in seedData)
        {
            var owner = users[ownerIndex];
            var listing = new Listing(title, description, price, category, condition, campus, owner.Id);
            listing.State = state;
            if (state == ListingState.Sold)
                listing.UpdatedAt = DateTime.UtcNow.AddDays(-3);

            context.Listings.Add(listing);
            listings.Add(listing);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
        return listings;
    }

    private static async Task SeedImagesAsync(ApplicationDbContext context, List<Listing> listings)
    {
        for (int i = 0; i < listings.Count; i++)
        {
            var listing = listings[i];
            var imageCount = i == 0 ? 5 : 3;
            var imageSeed = i * 10;

            for (int j = 0; j < imageCount; j++)
            {
                var url = $"https://picsum.photos/seed/listing{i}img{j}/800/600";
                var image = new ListingImage(url, listing.Id, isPrimary: j == 0, displayOrder: j,
                    altText: listing.Title);
                context.ListingImages.Add(image);
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static async Task SeedConversationsAndMessagesAsync(
        ApplicationDbContext context, List<User> users, List<Listing> listings)
    {
        var random = new Random(42);
        var chatDefs = new (int BuyerIdx, int ListingIdx, string FirstMessage)[]
        {
            (4, 0, "Hola, ¿aún tienes disponible la calculadora? Me interesa para un parcial la próxima semana."),
            (5, 0, "Sí, todavía está disponible. ¿Cuándo la necesitas?"),
            (6, 1, "¿El libro de Stewart tiene los códigos de acceso sin usar?"),
            (7, 2, "¿El monitor incluye cable HDMI?"),
            (3, 3, "Me interesa el escritorio, ¿podrías enviar más fotos?"),
            (4, 3, "Claro, te las envío por el chat. ¿Para cuándo lo necesitas?"),
            (5, 5, "¿El iPad incluye el Apple Pencil de verdad? ¿De qué generación es?"),
            (6, 5, "Sí, es el Apple Pencil de 2da generación. Todo en perfecto estado."),
            (7, 6, "Hola, ¿la bicicleta tiene cambios Shimano?"),
            (8, 6, "Sí, tiene cambios Shimano Deore de 12 velocidades. Muy suaves."),
            (2, 7, "¿Los audífonos tienen el estuche de carga original?"),
            (3, 7, "Sí, incluye estuche, cable USB-C y adaptador de avión."),
            (1, 8, "¿El libro de Cengel está actualizado a la última edición?"),
            (4, 9, "¿La silla tiene ajuste de altura? Mido 1.85m y necesito que sea alta."),
            (5, 9, "Sí, tiene ajuste neumático y va bastante alto. Sin problema."),
            (6, 10, "¿La camiseta es original o réplica?"),
            (7, 11, "¿Qué ciclo de batería tiene el MacBook? ¿Tiene AppleCare?"),
            (8, 11, "Tiene 230 ciclos y AppleCare hasta diciembre de este año."),
            (2, 12, "¿Los apuntes vienen en PDF o en formato físico?"),
            (0, 14, "¿La sudadera tiene el logo de Ingeniería o el general?"),
        };

        var conversationMap = new Dictionary<string, Conversation>();
        var messages = new List<Message>();

        for (int i = 0; i < chatDefs.Length; i++)
        {
            var (buyerIdx, listingIdx, firstMsg) = chatDefs[i];
            var buyer = users[buyerIdx];
            var listing = listings[listingIdx];
            var seller = listing.Owner;
            var key = $"{buyer.Id}-{listing.Id}";

            if (buyer.Id == seller.Id) continue;

            Conversation conversation;
            if (!conversationMap.TryGetValue(key, out var existingConv))
            {
                conversation = new Conversation(listing.Id, buyer.Id, seller.Id);
                context.Conversations.Add(conversation);
                conversationMap[key] = conversation;

                var msg = new Message(conversation.Id, buyer.Id, firstMsg);
                context.Messages.Add(msg);
                messages.Add(msg);
            }
            else
            {
                conversation = existingConv;
                var msg = new Message(conversation.Id, buyer.Id, firstMsg);
                context.Messages.Add(msg);
                messages.Add(msg);
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);

        var responses = new[]
        {
            "Gracias por tu interés. Avísame si tienes más preguntas.",
            "Perfecto, podemos coordinar la entrega en el campus.",
            "Lo tengo disponible. ¿Quieres verlo en persona?",
            "Sí, claro. Podemos encontrarnos en la cafetería del Bloque 18.",
            "El precio es negociable si lo recoges pronto.",
            "Está en muy buen estado, te va a gustar.",
            "Lo vendo porque ya no lo necesito este semestre.",
            "Acepto Nequi o efectivo, como prefieras.",
        };

        foreach (var conv in conversationMap.Values)
        {
            var response = responses[random.Next(responses.Length)];
            var responseMsg = new Message(conv.Id, conv.SellerId, response);
            context.Messages.Add(responseMsg);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static async Task SeedReportsAsync(
        ApplicationDbContext context, List<User> users, List<Listing> listings, User adminUser)
    {
        var reportDefs = new (int ReporterIdx, int ReportedUserIdx, int? ListingIdx, string Reason, string Comment, ReportStatus Status)[]
        {
            (3, 8, 0, "Comportamiento inapropiado", "El usuario fue grosero en el chat cuando pregunté por la calculadora.", ReportStatus.Open),
            (5, 8, null, "Spam", "Este usuario está enviando mensajes masivos promocionando productos externos.", ReportStatus.Open),
            (1, 6, 6, "Precio engañoso", "La bicicleta decía 750,000 en la publicación pero en el chat pidió 900,000.", ReportStatus.Open),
            (7, 5, 5, "Artículo no coincide", "El iPad que me mostró no era el mismo de las fotos. Tenía un rayón en la pantalla.", ReportStatus.Open),
            (2, 8, null, "Suplantación de identidad", "Este perfil está usando fotos que no son de él. Es un compañero que conozco.", ReportStatus.Open),
            (4, 3, 14, "Contenido inapropiado", "Las fotos de la sudadera muestran contenido que no debería estar en la plataforma.", ReportStatus.Open),
            (6, 8, null, "Acoso", "El usuario me ha enviado múltiples mensajes después de decirle que no estaba interesado.", ReportStatus.Open),
            (0, 7, 7, "Artículo defectuoso", "Los audífonos llegaron con la cancelación de ruido dañada y el vendedor no responde.", ReportStatus.Closed),
            (2, 8, null, "Cuenta falsa", "Sospecho que este usuario tiene múltiples cuentas para inflar sus calificaciones.", ReportStatus.Closed),
            (5, 4, 10, "Falsificación", "La camiseta que vende no es original de la selección. Es una réplica barata.", ReportStatus.Closed),
        };

        foreach (var (reporterIdx, reportedUserIdx, listingIdx, reason, comment, status) in reportDefs)
        {
            var reporter = users[reporterIdx];
            var reportedUser = users[reportedUserIdx];
            var reportedListingId = listingIdx.HasValue ? listings[listingIdx.Value].Id : (Guid?)null;

            if (reporter.Id == reportedUser.Id) continue;

            var report = new Report(reporter.Id, reportedUser.Id, reason, comment, reportedListingId);
            report.Status = status;

            if (status == ReportStatus.Closed)
            {
                report.ResolvedByUserId = adminUser.Id;
                report.ResolvedAt = DateTime.UtcNow.AddDays(-1);
                report.ResolutionNote = "Caso revisado por administración. Se tomaron las medidas correspondientes.";
            }

            context.Reports.Add(report);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}
