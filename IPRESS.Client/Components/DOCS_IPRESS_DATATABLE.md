# IpressDataTable – Componente unificado de tablas

Componente reutilizable que centraliza todas las capacidades de tabla (filtros, orden, agrupación, columnas fijas, detalle expandible) para usarse en **Diresas**, **Red**, **MicroRed**, **Establecimientos** y cualquier otro módulo que necesite una grilla.

---

## Funcionalidades disponibles

| Funcionalidad | Dónde se configura | Uso en módulos |
|--------------|--------------------|----------------|
| **Filtros por columna** | `AllowFiltering="true"` (por defecto) + en cada columna `Filterable="true"` | Todas las tablas: filtrar por cualquier columna. |
| **Modo de filtro** | `FilterMode`: `Simple`, `SimpleWithMenu`, `Advanced`, `CheckBoxList` | Por defecto `SimpleWithMenu`: filtro con menú de operador (contiene, empieza por, etc.). |
| **Ordenación por columna** | `AllowSorting="true"` (por defecto) + en cada columna `Sortable="true"` | Clic en cabecera para ordenar ascendente/descendente. |
| **Agrupación** | `AllowGrouping="true"` (por defecto) + en cada columna `Groupable="true"` | Arrastrar una columna al panel “Arrastre una columna aquí para agrupar” para agrupar (ej. Red por Diresa). |
| **Columnas fijas (congeladas)** | En la definición de columna: `Frozen="true"` | Fijar la primera (o varias) columnas al desplazar horizontalmente. |
| **Tabla dentro de tabla (master-detail)** | `DetailTemplate` con `RenderFragment<TItem>` | Filas expandibles con detalle (ej. cabecera Red y dentro una tabla de MicroRedes). |
| **Modo de expansión** | `ExpandMode`: `Single` (una fila abierta) o `Multiple` (varias) | Solo aplica si se usa `DetailTemplate`. |
| **Paginación** | `AllowPaging="true"`, `PageSize`, `ShowPagingSummary` | Por defecto 25 registros por página y resumen “Mostrando X a Y de Z”. |
| **Redimensionar columnas** | `AllowColumnResize="true"` (por defecto) | Arrastrar el borde del encabezado para cambiar ancho. |
| **Reordenar columnas** | `AllowColumnReorder="true"` (por defecto) | Arrastrar encabezado para cambiar orden de columnas. |
| **Texto vacío** | `EmptyText` | Mensaje cuando no hay datos (por defecto: “No hay registros.”). |

Todas las opciones son **parámetros** del componente: se pueden activar/desactivar o personalizar por módulo sin tocar el componente.

---

## Dónde y cómo se usa

### Ubicación del componente

- **Ruta:** `IPRESS.Client/Components/IpressDataTable.razor`
- **Nombre:** `IpressDataTable<TItem>`
- **Referencia al grid:** la propiedad pública `Grid` devuelve el `RadzenDataGrid<TItem>` interno (para llamar a `Reload()`, `GetFilteredData()`, etc. si se necesita).

### Uso básico (sin detalle, sin columnas fijas)

Las columnas se pasan con el parámetro **Columns** (dentro de `<Columns>`). Para master-detail (tabla dentro de tabla), usar `RadzenDataGrid` directamente en la página con su `DetailTemplate`.

```razor
<IpressDataTable TItem="DiresaDto" Data="@items" Style="margin-top:0.5rem;flex:1">
    <Columns>
        <RadzenDataGridColumn TItem="DiresaDto" Property="Codigo" Title="Código" Width="120px" />
        <RadzenDataGridColumn TItem="DiresaDto" Property="Nombre" Title="Nombre" />
        <RadzenDataGridColumn TItem="DiresaDto" Property="Ubigeo" Title="Ubigeo" Width="100px" />
        <RadzenDataGridColumn TItem="DiresaDto" Title="Acciones" Sortable="false" Filterable="false" Width="140px">
            <Template Context="d">...</Template>
        </RadzenDataGridColumn>
    </Columns>
</IpressDataTable>
```

Por defecto ya tiene: filtros por columna, orden, agrupación, paginación, redimensionar y reordenar columnas.

### Desactivar agrupación o filtros en un módulo

```razor
<IpressDataTable TItem="MiDto" Data="@items" AllowGrouping="false" AllowFiltering="false">
    <Columns>...</Columns>
</IpressDataTable>
```

### Columnas fijas (fijar primera columna)

En la definición de columnas, se pone `Frozen="true"` en las columnas que deban quedar fijas al hacer scroll horizontal:

```razor
<IpressDataTable TItem="RedDto" Data="@items">
    <Columns>
        <RadzenDataGridColumn TItem="RedDto" Property="Diresa" Title="Diresa" Frozen="true" Width="140px" />
        <RadzenDataGridColumn TItem="RedDto" Property="Codigo" Title="Código" Width="100px" />
        ...
    </Columns>
</IpressDataTable>
```

### Tabla dentro de tabla (master-detail)

Se añade **DetailTemplate** como hermano de **Columns** dentro de `IpressDataTable`. El contexto de la fila se usa con `Context="row"` (o el nombre que prefieras):

```razor
<IpressDataTable TItem="RedDto" Data="@items" ExpandMode="DataGridExpandMode.Multiple">
    <Columns>
        <RadzenDataGridColumn TItem="RedDto" Property="Diresa" Title="Diresa" />
        <RadzenDataGridColumn TItem="RedDto" Property="Codigo" Title="Código" />
        <RadzenDataGridColumn TItem="RedDto" Property="Nombre" Title="Nombre" />
    </Columns>
    <DetailTemplate Context="row">
        <p><strong>Detalle de @row.Nombre</strong></p>
        <IpressDataTable TItem="MicroRedDto" Data="@itemsMicroRed.Where(m => m.IdRed == row.IdRed)" AllowGrouping="false">
            <Columns>
                <RadzenDataGridColumn TItem="MicroRedDto" Property="Codigo" Title="Código" />
                <RadzenDataGridColumn TItem="MicroRedDto" Property="Nombre" Title="Nombre" />
            </Columns>
        </IpressDataTable>
    </DetailTemplate>
</IpressDataTable>
```

- **Cabecera:** filas del `Data` principal (ej. Red).
- **Detalle:** al expandir una fila se muestra el contenido de `DetailTemplate`; `row` es la fila actual (ej. tabla de MicroRedes de esa Red).

### Resumen por módulo

| Módulo | Uso de IpressDataTable | Opciones típicas |
|--------|------------------------|------------------|
| **Diresas** | Listado principal | Filtros, orden, agrupación, paginación (todo por defecto). |
| **Red** | Listado principal | Igual + opcional columna Diresa `Frozen="true"`. |
| **MicroRed** | Listado principal | Igual + opcional columna Red `Frozen="true"`. |
| **Establecimientos** | Listado principal | Igual; opcional `DetailTemplate` para Centros Poblados u otro detalle. |
| **Cualquier otro** | Donde se necesite una grilla | Activar/desactivar solo las funciones que se quieran. |

---

## Parámetros del componente (resumen)

- **Data**, **Columns**, **DetailTemplate** (opcional).  
- **AllowFiltering**, **AllowSorting**, **AllowPaging**, **AllowGrouping** (por defecto `true`).  
- **FilterMode**, **FilterCaseSensitivity**, **GroupPanelText**.  
- **PageSize**, **ShowPagingSummary**, **PagingSummaryFormat**, **PagerHorizontalAlign**.  
- **ExpandMode** (para detalle).  
- **AllowColumnResize**, **AllowColumnReorder**.  
- **Compact**, **Style**, **Class**, **EmptyText**.  

Para no usar una funcionalidad en un módulo concreto, se pasa el parámetro correspondiente en `false` o se ajusta el contenido de `Columns` (por ejemplo quitando `Groupable` o poniendo `Frozen="false"`).

---

## Omitir funcionalidades en un módulo

Cualquier capacidad se puede desactivar por página sin cambiar el componente:

| Si no quieres…           | Haz esto en la página |
|-------------------------|------------------------|
| Agrupación              | `AllowGrouping="false"` |
| Filtros                 | `AllowFiltering="false"` |
| Ordenación              | `AllowSorting="false"` |
| Paginación              | `AllowPaging="false"` |
| Redimensionar columnas  | `AllowColumnResize="false"` |
| Reordenar columnas      | `AllowColumnReorder="false"` |
| Fijar una columna       | No uses `Frozen="true"` en esa columna (o pon `Frozen="false"`). |

---

## Personalizar la paginación

### Comportamiento (cantidad, texto, alineación)

En **`IPRESS.Client/Components/IpressDataTable.razor`** (líneas ~99–115) puedes modificar:

| Parámetro | Descripción | Ejemplo |
|-----------|-------------|---------|
| `AllowPaging` | Activar/desactivar paginación | `true` / `false` |
| `PageSize` | Registros por página por defecto | `20`, `25`, `50` |
| `PageSizeOptions` | Valores del selector "Registros por página" | `new[] { 10, 20, 50, 100 }` |
| `ShowPagingSummary` | Mostrar texto "Mostrando X a Y de Z…" | `true` / `false` |
| `PagingSummaryFormat` | Formato del resumen (`{0}` primero, `{1}` último, `{2}` total) | `"Mostrando {0} a {1} de {2} registros"` |
| `PagerHorizontalAlign` | Alineación del bloque de paginación | `HorizontalAlign.Left/Center/Right` |

### Estilos (colores, tamaños, bordes)

En **`IPRESS.Client/wwwroot/css/ipress-theme.css`** busca el comentario:

```text
/* ========== PAGINACIÓN (personalizable desde aquí) ==========
```

Ahí están los estilos del contenedor (`.rz-datatable .rz-paginator`), botones de navegación (<<, <, >, >>), números de página, página actual (`.rz-state-active`), selector de tamaño y texto del resumen. Puedes cambiar colores, `padding`, `border-radius`, etc. en ese bloque.

---

## Referencia al grid interno

Si necesitas llamar métodos del grid (por ejemplo `Reload()` o leer datos filtrados):

```razor
<IpressDataTable @ref="miTabla" TItem="DiresaDto" Data="@items">
    <Columns>...</Columns>
</IpressDataTable>

@code {
    private IpressDataTable<DiresaDto>? miTabla;

    private void Algo()
    {
        miTabla?.Grid?.Reload();
    }
}
```
