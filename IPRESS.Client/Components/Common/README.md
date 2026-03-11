# Componentes comunes IPRESS

Componentes reutilizables para mantener **un solo estilo** en todo el sistema (formularios de Red, Diresa, MicroRed, Establecimiento, Centro Poblado, Usuarios, etc.). Si se actualiza un componente aquí, el cambio se refleja en todas las páginas que lo usan.

## Componentes

| Componente | Uso | Estilo aplicado |
|------------|-----|-----------------|
| **IpressFormField** | Envuelve etiqueta + control. | `Variant="Outlined"`, `AllowFloatingLabel="false"`, etiqueta arriba del input. |
| **IpressTextBox** | Input de texto. | `Class="ipress-input-equal"`, `Style="width:100%"`, altura 32px (vía CSS global). |
| **IpressDropDown** | Selector (combo). | Mismo alto que inputs. Genérico: `<IpressDropDown TValue="int" @bind-Value="id" Data="..." TextProperty="Nombre" ValueProperty="Id" />`. |
| **IpressNumeric** | Input numérico. | Misma altura y ancho 100%. |

## Uso

En formularios (Red, Diresa, etc.) usar estos componentes en lugar de `RadzenFormField`/`RadzenTextBox`/`RadzenDropDown` para que el aspecto sea el mismo que en Establecimiento de Salud y que un cambio en `ipress-theme.css` o en el componente afecte a todo el sistema.

Ejemplo (Red / Crear):

```razor
<IpressFormField Text="Diresa">
    <IpressDropDown TValue="int" @bind-Value="Form.IdDiresa" Data="@diresasConOpcion" TextProperty="Nombre" ValueProperty="Id" />
</IpressFormField>
<IpressFormField Text="Código">
    <IpressTextBox @bind-Value="@CodigoTexto" Placeholder="" MaxLength="10" />
</IpressFormField>
```

## Altura unificada

El archivo `wwwroot/css/ipress-theme.css` define la altura de **todos** los inputs (32px) a nivel global (`.rz-inputtext`, `.rz-dropdown`, `.rz-textbox`, etc.), de modo que incluso los formularios que aún usan directamente Radzen se vean con el mismo alto.
