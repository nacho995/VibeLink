namespace VibeLink.App.Helpers;

/// <summary>
/// DataTemplateSelector para el chat: elige un template diferente
/// según si el mensaje es mío (enviado) o del otro usuario (recibido).
/// </summary>
public class ChatBubbleSelector : DataTemplateSelector
{
    public DataTemplate? SentTemplate { get; set; }
    public DataTemplate? ReceivedTemplate { get; set; }

    /// <summary>
    /// El userId del usuario actual se pasa via BindingContext de la página.
    /// Comparamos el UserId del mensaje con el del usuario actual.
    /// </summary>
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is Models.MessageChat message)
        {
            // Obtenemos el ViewModel del CollectionView para saber quién es el usuario actual
            var page = GetParentPage(container);
            if (page?.BindingContext is ViewModels.ChatViewModel vm)
            {
                // Si el UserId del mensaje es distinto al matchUserId,
                // quiere decir que el mensaje lo envió el usuario actual
                // (porque userId siempre es quien envía)
                return SentTemplate ?? ReceivedTemplate!;
            }
        }
        return ReceivedTemplate!;
    }

    private static Page? GetParentPage(BindableObject? obj)
    {
        while (obj != null)
        {
            if (obj is Page page) return page;
            obj = (obj as Element)?.Parent;
        }
        return null;
    }
}
