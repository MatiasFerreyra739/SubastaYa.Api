function renderNavbar() {
    const container = document.getElementById('navbar-container');
    if (!container) return;

    const userId = getUserId();

    container.innerHTML = `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <div class="container">
        <a class="navbar-brand fw-bold" href="index.html">
          <i class="bi bi-hammer"></i> SubastaYa
        </a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMain">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navMain">
          <ul class="navbar-nav me-auto">
            <li class="nav-item"><a class="nav-link" href="index.html">Catálogo</a></li>
            <li class="nav-item"><a class="nav-link" href="publicar.html">Publicar</a></li>
            <li class="nav-item"><a class="nav-link" href="billetera.html">Billetera</a></li>
            <li class="nav-item"><a class="nav-link" href="mis-actividades.html">Mis actividades</a></li>
          </ul>
          <div class="d-flex align-items-center gap-2">
            <span class="text-light small">Usuario:</span>
            <select id="selector-usuario" class="form-select form-select-sm" style="width:auto;">
              <option value="1">vendedor@test.com</option>
              <option value="2">comprador1@test.com</option>
              <option value="3">comprador2@test.com</option>
              <option value="4">sinfondos@test.com</option>
            </select>
          </div>
        </div>
      </div>
    </nav>`;

    const selector = document.getElementById('selector-usuario');
    selector.value = userId;
    selector.addEventListener('change', (e) => {
        setUserId(e.target.value);
        mostrarToast(`Usuario cambiado a ${e.target.options[e.target.selectedIndex].text}`, 'info');
        setTimeout(() => location.reload(), 400);
    });
}

document.addEventListener('DOMContentLoaded', renderNavbar);