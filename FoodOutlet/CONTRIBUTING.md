# Contributing Guidelines

## Project Structure
- **LayoutWeb.cshtml**: Navbar template for table view - contains logo, search bar, filter button, and menu
- **table.cshtml**: Content page - displays food items in responsive grid layout
- **CSS**: Full-width responsive design using Bootstrap container-fluid
- **JavaScript**: Filter panel toggle and search functionality

## Coding Standards

### Layout Guidelines
- Use `container-fluid` for full-width layouts
- Remove all sidebar spacing and left margins
- All pages should extend LayoutWeb.cshtml for consistent navbar
- Maintain full viewport width (100vw) without scrollbars

### Component Requirements
- **Navbar**: Red (#c0100b) background, logo on left, search center, filter/menu right
- **Filter Panel**: Slide-in from right, categories and price ranges with toggle buttons
- **Food Cards**: 2-column grid, centered, with rounded corners and shadows
- **Responsive**: Mobile-first approach, collapse to 1 column on small screens

### File Organization
- Layout files in `Views/Shared/`
- Content pages in `Views/Entry/` or respective controllers
- Custom CSS in `wwwroot/css/custom.css`
- JavaScript for interactions in `wwwroot/js/custom.js`

### Naming Conventions
- CSS classes: kebab-case (e.g., `filter-panel`, `food-card`)
- IDs: camelCase (e.g., `filterId`, `searchForm`)
- Bootstrap classes: Use Bootstrap 4/5 utility classes