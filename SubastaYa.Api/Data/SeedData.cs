
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(
            ApplicationDbContext context)
        {
            // Si ya existen usuarios, no volvemos a cargar
            // los datos iniciales.
            if (await context.Usuarios.AnyAsync())
                return;

            // =========================
            // CATEGORÍAS
            // =========================

            var tecnologia = new Categoria
            {
                Id = 1,
                nombre = "Tecnología",
                url_icono = ""
            };

            var coleccionables = new Categoria
            {
                Id = 2,
                nombre = "Coleccionables",
                url_icono = ""
            };

            var indumentaria = new Categoria
            {
                Id = 3,
                nombre = "Indumentaria",
                url_icono = ""
            };

            var vehiculos = new Categoria
            {
                Id = 4,
                nombre = "Vehículos",
                url_icono = ""
            };

            context.Categorias.AddRange(
                tecnologia,
                coleccionables,
                indumentaria,
                vehiculos);

            // =========================
            // USUARIOS
            // =========================

            var vendedor = new Usuario
            {
                id = 1,
                email = "vendedor@test.com",
                nombre = "Vendedor",
                password_hash = "hash-prueba",
                fecha_registro = DateTime.Now
            };

            var comprador1 = new Usuario
            {
                id = 2,
                email = "comprador1@test.com",
                nombre = "Comprador 1",
                password_hash = "hash-prueba",
                fecha_registro = DateTime.Now
            };

            var comprador2 = new Usuario
            {
                id = 3,
                email = "comprador2@test.com",
                nombre = "Comprador 2",
                password_hash = "hash-prueba",
                fecha_registro = DateTime.Now
            };

            var sinFondos = new Usuario
            {
                id = 4,
                email = "sinfondos@test.com",
                nombre = "Sin Fondos",
                password_hash = "hash-prueba",
                fecha_registro = DateTime.Now
            };

            context.Usuarios.AddRange(
                vendedor,
                comprador1,
                comprador2,
                sinFondos);

            // =========================
            // BILLETERAS
            // =========================

            var billeteraVendedor = new Billetera
            {
                id = 1,
                usuario_id = vendedor.id,
                saldo_total = 0,
                saldo_retenido = 0,
                saldo_disponible = 0,
                version = 0
            };

            var billeteraComprador1 = new Billetera
            {
                id = 2,
                usuario_id = comprador1.id,
                saldo_total = 150000,
                saldo_retenido = 45000,
                saldo_disponible = 105000,
                version = 0
            };

            var billeteraComprador2 = new Billetera
            {
                id = 3,
                usuario_id = comprador2.id,
                saldo_total = 200000,
                saldo_retenido = 0,
                saldo_disponible = 200000,
                version = 0
            };

            var billeteraSinFondos = new Billetera
            {
                id = 4,
                usuario_id = sinFondos.id,
                saldo_total = 500,
                saldo_retenido = 0,
                saldo_disponible = 500,
                version = 0
            };

            context.Billeteras.AddRange(
                billeteraVendedor,
                billeteraComprador1,
                billeteraComprador2,
                billeteraSinFondos);

            // =========================
            // SUBASTAS
            // =========================

            var ahora = DateTime.Now;

            // ---------------------------------
            // SUBASTA 1
            // Activa normal
            // Dos pujas
            // Líder actual: comprador1 $45.000
            // ---------------------------------

            var subasta1 = new Subasta
            {
                id = 1,
                vendedor_id = vendedor.id,
                categoria_id = tecnologia.Id,
                titulo = "Notebook Lenovo",
                descripcion = "Notebook Lenovo en buen estado",
                url_imagen = "",
                precio_base = 35000,
                incremento_minimo = 5000,
                fecha_inicio = ahora.AddMinutes(-10),
                fecha_fin = ahora.AddMinutes(25),
                estado = "ACTIVA",
                version = 2
            };

            // ---------------------------------
            // SUBASTA 2
            // Activa crítica
            // Finaliza en menos de 2 minutos
            // ---------------------------------

            var subasta2 = new Subasta
            {
                id = 2,
                vendedor_id = vendedor.id,
                categoria_id = tecnologia.Id,
                titulo = "Notebook ThinkPad",
                descripcion = "Notebook usada en muy buen estado",
                url_imagen = "",
                precio_base = 110000,
                incremento_minimo = 5000,
                fecha_inicio = ahora.AddMinutes(-10),
                fecha_fin = ahora.AddMinutes(1),
                estado = "ACTIVA",
                version = 0
            };

            // ---------------------------------
            // SUBASTA 3
            // Próxima. Comienza dentro de 24 horas.
            // ---------------------------------

            var subasta3 = new Subasta
            {
                id = 3,
                vendedor_id = vendedor.id,
                categoria_id = coleccionables.Id,
                titulo = "Colección de monedas",
                descripcion = "Colección de monedas antiguas",
                url_imagen = "",
                precio_base = 50000,
                incremento_minimo = 2500,
                fecha_inicio = ahora.AddDays(1),
                fecha_fin = ahora.AddDays(2),
                estado = "ACTIVA",
                version = 0
            };

            // ---------------------------------
            // SUBASTA 4
            // Vencida con ganador.
            // Se conserva como escenario histórico.
            // ---------------------------------

            var subasta4 = new Subasta
            {
                id = 4,
                vendedor_id = vendedor.id,
                categoria_id = indumentaria.Id,
                titulo = "Campera de colección",
                descripcion = "Campera en excelente estado",
                url_imagen = "",
                precio_base = 30000,
                incremento_minimo = 1000,
                fecha_inicio = ahora.AddHours(-2),
                fecha_fin = ahora.AddMinutes(-10),
                estado = "FINALIZADA",
                version = 1
            };

            // ---------------------------------
            // SUBASTA 5
            // Vencida desierta.
            // ---------------------------------

            var subasta5 = new Subasta
            {
                id = 5,
                vendedor_id = vendedor.id,
                categoria_id = vehiculos.Id,
                titulo = "Bicicleta",
                descripcion = "Bicicleta usada",
                url_imagen = "",
                precio_base = 80000,
                incremento_minimo = 5000,
                fecha_inicio = ahora.AddHours(-2),
                fecha_fin = ahora.AddMinutes(-10),
                estado = "DESIERTA",
                version = 1
            };

            context.Subastas.AddRange(
                subasta1,
                subasta2,
                subasta3,
                subasta4,
                subasta5);

            // =========================
            // PUJAS
            // =========================

            // Primera puja de la subasta 1.
            // Comprador2 ofrece $40.000.
            var puja1 = new Puja
            {
                id = 1,
                subasta_id = subasta1.id,
                comprador_id = comprador2.id,
                monto = 40000,
                fecha_puja = ahora.AddMinutes(-8)
            };

            // Segunda puja de la subasta 1.
            // Comprador1 supera la anterior con $45.000.
            var puja2 = new Puja
            {
                id = 2,
                subasta_id = subasta1.id,
                comprador_id = comprador1.id,
                monto = 45000,
                fecha_puja = ahora.AddMinutes(-5)
            };

            // Puja histórica de la subasta 4.
            // Se utiliza para representar el escenario
            // "vencida con ganador".
            var puja3 = new Puja
            {
                id = 3,
                subasta_id = subasta4.id,
                comprador_id = comprador2.id,
                monto = 30000,
                fecha_puja = ahora.AddHours(-1)
            };

            context.Pujas.AddRange(
                puja1,
                puja2,
                puja3);

            // =========================
            // TRANSACCIONES LEDGER
            // =========================

            // Depósito inicial del comprador1.
            var depositoComprador1 = new Transaccion_Ledger
            {
                id = 1,
                billetera_id = billeteraComprador1.id,
                tipo = "DEPOSITO",
                monto = 150000,
                fecha = ahora.AddHours(-3),
                subasta_id = null
            };

            // Depósito inicial del comprador2.
            var depositoComprador2 = new Transaccion_Ledger
            {
                id = 2,
                billetera_id = billeteraComprador2.id,
                tipo = "DEPOSITO",
                monto = 200000,
                fecha = ahora.AddHours(-3),
                subasta_id = null
            };

            // Depósito inicial del usuario sin fondos.
            var depositoSinFondos = new Transaccion_Ledger
            {
                id = 3,
                billetera_id = billeteraSinFondos.id,
                tipo = "DEPOSITO",
                monto = 500,
                fecha = ahora.AddHours(-3),
                subasta_id = null
            };

            // Comprador2 retuvo $40.000 con su primera puja.
            var retencionComprador2 = new Transaccion_Ledger
            {
                id = 4,
                billetera_id = billeteraComprador2.id,
                tipo = "RETENCION",
                monto = 40000,
                fecha = ahora.AddMinutes(-8),
                subasta_id = subasta1.id
            };

            // Comprador2 queda fuera de la puja.
            // Se liberan sus $40.000.
            var liberacionComprador2 = new Transaccion_Ledger
            {
                id = 5,
                billetera_id = billeteraComprador2.id,
                tipo = "LIBERACION",
                monto = 40000,
                fecha = ahora.AddMinutes(-5),
                subasta_id = subasta1.id
            };

            // Comprador1 pasa a ser líder.
            // Sus $45.000 quedan retenidos.
            var retencionComprador1 = new Transaccion_Ledger
            {
                id = 6,
                billetera_id = billeteraComprador1.id,
                tipo = "RETENCION",
                monto = 45000,
                fecha = ahora.AddMinutes(-5),
                subasta_id = subasta1.id
            };

            context.Transacciones_Ledgers.AddRange(
                depositoComprador1,
                depositoComprador2,
                depositoSinFondos,
                retencionComprador2,
                liberacionComprador2,
                retencionComprador1);

            // =========================
            // AUDITORÍA
            // =========================

            var auditoriaSubasta4 = new Auditoria_Log
            {
                id = 1,
                entidad = "SUBASTA",
                entidad_id = subasta4.id,
                accion = "SUBASTA_FINALIZADA",
                usuario_id = comprador2.id,
                detalle_json =
                    "{\"comprador_id\":3,\"monto\":30000}",
                fecha = ahora.AddMinutes(-9)
            };

            var auditoriaSubasta5 = new Auditoria_Log
            {
                id = 2,
                entidad = "SUBASTA",
                entidad_id = subasta5.id,
                accion = "SUBASTA_DESIERTA",
                usuario_id = null,
                detalle_json =
                    "{\"motivo\":\"SIN_PUJAS\"}",
                fecha = ahora.AddMinutes(-9)
            };

            context.Auditorias_Log.AddRange(
                auditoriaSubasta4,
                auditoriaSubasta5);

            // =========================
            // GUARDAR
            // =========================

            await context.SaveChangesAsync();
        }
    }
}

