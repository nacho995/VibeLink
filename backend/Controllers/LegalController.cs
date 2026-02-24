using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Páginas legales requeridas por las app stores.
/// </summary>
[ApiController]
public class LegalController : ControllerBase
{
    [HttpGet("/privacy")]
    [Produces("text/html")]
    public ContentResult Privacy()
    {
        var html = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Política de Privacidad - VibeLink</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; 
               max-width: 800px; margin: 0 auto; padding: 20px; line-height: 1.6;
               background: #1a1a2e; color: #e0e0e0; }
        h1 { color: #e94560; }
        h2 { color: #00d4ff; margin-top: 30px; }
        a { color: #7bed9f; }
    </style>
</head>
<body>
    <h1>Política de Privacidad de VibeLink</h1>
    <p><strong>Última actualización:</strong> Febrero 2024</p>

    <h2>1. Información que Recopilamos</h2>
    <p>Recopilamos la siguiente información cuando usas VibeLink:</p>
    <ul>
        <li><strong>Información de cuenta:</strong> email, nombre de usuario, contraseña (encriptada)</li>
        <li><strong>Perfil:</strong> foto (opcional), biografía, edad, ubicación general</li>
        <li><strong>Preferencias:</strong> películas, series y videojuegos que te gustan o no</li>
        <li><strong>Interacciones:</strong> matches, mensajes de chat, swipes</li>
    </ul>

    <h2>2. Cómo Usamos tu Información</h2>
    <ul>
        <li>Para crear y gestionar tu cuenta</li>
        <li>Para encontrar usuarios compatibles basándonos en tus gustos</li>
        <li>Para permitir la comunicación entre matches</li>
        <li>Para mejorar nuestro algoritmo de compatibilidad</li>
        <li>Para enviarte notificaciones sobre matches y mensajes</li>
    </ul>

    <h2>3. Compartición de Datos</h2>
    <p>NO vendemos tus datos personales. Compartimos información solo:</p>
    <ul>
        <li>Con otros usuarios: solo tu perfil público y gustos compartidos</li>
        <li>Con proveedores de servicios: Stripe (pagos), servidores cloud</li>
        <li>Por requerimiento legal: si la ley lo exige</li>
    </ul>

    <h2>4. Seguridad</h2>
    <p>Protegemos tus datos mediante:</p>
    <ul>
        <li>Encriptación de contraseñas (BCrypt)</li>
        <li>Conexiones HTTPS</li>
        <li>Tokens JWT para autenticación</li>
        <li>Servidores seguros en la Unión Europea</li>
    </ul>

    <h2>5. Tus Derechos (RGPD)</h2>
    <p>Tienes derecho a:</p>
    <ul>
        <li>Acceder a tus datos personales</li>
        <li>Rectificar información incorrecta</li>
        <li>Eliminar tu cuenta y todos tus datos</li>
        <li>Exportar tus datos</li>
        <li>Oponerte al procesamiento</li>
    </ul>

    <h2>6. Eliminación de Cuenta</h2>
    <p>Puedes eliminar tu cuenta en cualquier momento desde la configuración de la app. 
    Esto eliminará permanentemente todos tus datos, matches y mensajes.</p>

    <h2>7. Menores de Edad</h2>
    <p>VibeLink está destinado a mayores de 18 años. No recopilamos intencionadamente 
    información de menores.</p>

    <h2>8. Cambios en esta Política</h2>
    <p>Podemos actualizar esta política. Te notificaremos de cambios significativos 
    a través de la app o por email.</p>

    <h2>9. Contacto</h2>
    <p>Para preguntas sobre privacidad:</p>
    <p>Email: <a href='mailto:privacy@vibelink.app'>privacy@vibelink.app</a></p>

    <p style='margin-top: 50px; color: #666;'>© 2024 VibeLink. Todos los derechos reservados.</p>
</body>
</html>";
        return Content(html, "text/html");
    }

    [HttpGet("/terms")]
    [Produces("text/html")]
    public ContentResult Terms()
    {
        var html = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Términos de Uso - VibeLink</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; 
               max-width: 800px; margin: 0 auto; padding: 20px; line-height: 1.6;
               background: #1a1a2e; color: #e0e0e0; }
        h1 { color: #e94560; }
        h2 { color: #00d4ff; margin-top: 30px; }
    </style>
</head>
<body>
    <h1>Términos de Uso de VibeLink</h1>
    <p><strong>Última actualización:</strong> Febrero 2024</p>

    <h2>1. Aceptación</h2>
    <p>Al usar VibeLink, aceptas estos términos de uso.</p>

    <h2>2. Elegibilidad</h2>
    <p>Debes tener al menos 18 años para usar VibeLink.</p>

    <h2>3. Tu Cuenta</h2>
    <ul>
        <li>Eres responsable de mantener tu contraseña segura</li>
        <li>No puedes crear cuentas falsas o suplantar identidades</li>
        <li>Una cuenta por persona</li>
    </ul>

    <h2>4. Conducta</h2>
    <p>No está permitido:</p>
    <ul>
        <li>Acosar o intimidar a otros usuarios</li>
        <li>Publicar contenido ilegal u ofensivo</li>
        <li>Spam o publicidad no autorizada</li>
        <li>Usar bots o automatización</li>
    </ul>

    <h2>5. Suscripción Premium</h2>
    <ul>
        <li>Los pagos se procesan de forma segura</li>
        <li>Puedes cancelar en cualquier momento</li>
        <li>No hay reembolsos por períodos parciales</li>
    </ul>

    <h2>6. Contenido</h2>
    <p>Conservas los derechos sobre tu contenido, pero nos das licencia para 
    mostrarlo en la plataforma.</p>

    <h2>7. Terminación</h2>
    <p>Podemos suspender cuentas que violen estos términos.</p>

    <h2>8. Limitación de Responsabilidad</h2>
    <p>VibeLink no es responsable de las interacciones entre usuarios fuera de la plataforma.</p>

    <h2>9. Contacto</h2>
    <p>Email: legal@vibelink.app</p>

    <p style='margin-top: 50px; color: #666;'>© 2024 VibeLink. Todos los derechos reservados.</p>
</body>
</html>";
        return Content(html, "text/html");
    }

    [HttpGet("/support")]
    [Produces("text/html")]
    public ContentResult Support()
    {
        var html = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Soporte - VibeLink</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; 
               max-width: 800px; margin: 0 auto; padding: 20px; line-height: 1.6;
               background: #1a1a2e; color: #e0e0e0; }
        h1 { color: #e94560; }
        h2 { color: #00d4ff; margin-top: 30px; }
        a { color: #7bed9f; }
        .contact { background: #16213e; padding: 20px; border-radius: 10px; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>Soporte de VibeLink</h1>

    <h2>Preguntas Frecuentes</h2>
    
    <h3>¿Cómo funciona el matching?</h3>
    <p>Nuestro algoritmo calcula la compatibilidad basándose en las películas, series 
    y videojuegos que ambos habéis marcado como favoritos.</p>

    <h3>¿Cuántos swipes tengo al día?</h3>
    <p>Los usuarios gratuitos tienen 10 swipes diarios. Premium tiene swipes ilimitados.</p>

    <h3>¿Cómo elimino mi cuenta?</h3>
    <p>Ve a Perfil > Configuración > Eliminar cuenta.</p>

    <h3>¿Cómo cancelo Premium?</h3>
    <p>Las suscripciones se gestionan a través de tu App Store (iOS) o Google Play (Android).</p>

    <div class='contact'>
        <h2>Contacto</h2>
        <p>¿No encuentras lo que buscas?</p>
        <p>Email: <a href='mailto:soporte@vibelink.app'>soporte@vibelink.app</a></p>
        <p>Respondemos en menos de 24 horas.</p>
    </div>

    <p style='margin-top: 50px; color: #666;'>© 2024 VibeLink. Todos los derechos reservados.</p>
</body>
</html>";
        return Content(html, "text/html");
    }
}
