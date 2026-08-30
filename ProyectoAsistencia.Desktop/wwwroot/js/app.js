// ============================================================
// Comunicación con el C# (Puente.cs) a través del WebView2.
// Cada mensaje que se manda lleva "accion"; la respuesta llega
// por el evento 'message' con la misma forma { accion: "...Resultado", ... }.
// ============================================================

let usuarioActual = null;

function enviarAlHost(mensaje) {
  window.chrome.webview.postMessage(mensaje);
}

window.chrome.webview.addEventListener('message', (evento) => {
  const datos = evento.data;
  switch (datos.accion) {
    case 'loginResultado': manejarLoginResultado(datos); break;
    case 'listarUsuariosResultado': manejarListarUsuariosResultado(datos); break;
    case 'crearUsuarioResultado': manejarCrearOModificarResultado(datos); break;
    case 'modificarUsuarioResultado': manejarCrearOModificarResultado(datos); break;
    case 'eliminarUsuarioResultado': manejarEliminarResultado(datos); break;
    case 'marcarEntradaResultado': mostrarMensajeAsistencia(datos); break;
    case 'marcarSalidaResultado': mostrarMensajeAsistencia(datos); break;
    case 'error': console.error('Error del host:', datos.mensaje); break;
  }
});

// ============================================================
// Navegación entre pantallas
// ============================================================

function mostrarPantalla(idPantalla) {
  document.querySelectorAll('.pantalla').forEach(el => el.classList.add('oculto'));
  document.getElementById(idPantalla).classList.remove('oculto');
}

function actualizarBarraSuperior() {
  const infoUsuario = document.getElementById('infoUsuario');
  if (!usuarioActual) {
    infoUsuario.classList.add('oculto');
    return;
  }
  infoUsuario.classList.remove('oculto');
  document.getElementById('nombreUsuario').textContent =
    `${usuarioActual.nombre} ${usuarioActual.apellido} (${usuarioActual.rol})`;
}

// ============================================================
// LOGIN
// ============================================================

document.getElementById('formLogin').addEventListener('submit', (e) => {
  e.preventDefault();
  const email = document.getElementById('loginEmail').value.trim();
  const password = document.getElementById('loginPassword').value;
  enviarAlHost({ accion: 'login', email, password });
});

function manejarLoginResultado(datos) {
  const mensajeEl = document.getElementById('loginMensaje');

  if (!datos.ok) {
    mensajeEl.textContent = datos.mensaje;
    mensajeEl.className = 'mensaje error';
    return;
  }

  mensajeEl.textContent = '';
  usuarioActual = datos.usuario;
  actualizarBarraSuperior();

  if (usuarioActual.rol === 'ADMINISTRADOR') {
    mostrarPantalla('pantallaAdmin');
    enviarAlHost({ accion: 'listarUsuarios' });
  } else {
    mostrarPantalla('pantallaEmpleado');
  }
}

document.getElementById('btnCerrarSesion').addEventListener('click', () => {
  usuarioActual = null;
  document.getElementById('formLogin').reset();
  document.getElementById('loginMensaje').textContent = '';
  actualizarBarraSuperior();
  mostrarPantalla('pantallaLogin');
});

// ============================================================
// PANEL EMPLEADO (CA-01)
// ============================================================

document.getElementById('btnMarcarEntrada').addEventListener('click', () => {
  enviarAlHost({ accion: 'marcarEntrada', idUsuario: usuarioActual.idUsuario });
});

document.getElementById('btnMarcarSalida').addEventListener('click', () => {
  enviarAlHost({ accion: 'marcarSalida', idUsuario: usuarioActual.idUsuario });
});

function mostrarMensajeAsistencia(datos) {
  const mensajeEl = document.getElementById('asistenciaMensaje');
  mensajeEl.textContent = datos.mensaje;
  mensajeEl.className = 'mensaje ' + (datos.ok ? 'exito' : 'error');
}

// ============================================================
// PANEL ADMINISTRADOR (GU-01, GU-02, GU-03)
// ============================================================

let usuariosCache = [];

function manejarListarUsuariosResultado(datos) {
  usuariosCache = datos.usuarios || [];
  const cuerpo = document.getElementById('cuerpoTablaUsuarios');
  cuerpo.innerHTML = '';

  usuariosCache.forEach(u => {
    const fila = document.createElement('tr');
    fila.innerHTML = `
      <td>${escaparHtml(u.nombre)} ${escaparHtml(u.apellido)}</td>
      <td>${escaparHtml(u.email)}</td>
      <td><span class="etiqueta-rol">${u.rol}</span></td>
      <td>${u.activo ? 'Activo' : 'Inactivo'}</td>
      <td class="acciones-fila">
        <button class="boton boton-secundario" data-accion="editar" data-id="${u.idUsuario}">Editar</button>
        <button class="boton boton-peligro" data-accion="eliminar" data-id="${u.idUsuario}">Eliminar</button>
      </td>
    `;
    cuerpo.appendChild(fila);
  });

  cuerpo.querySelectorAll('[data-accion="editar"]').forEach(boton => {
    boton.addEventListener('click', () => abrirModalEdicion(Number(boton.dataset.id)));
  });
  cuerpo.querySelectorAll('[data-accion="eliminar"]').forEach(boton => {
    boton.addEventListener('click', () => confirmarEliminacion(Number(boton.dataset.id)));
  });
}

function escaparHtml(texto) {
  const div = document.createElement('div');
  div.textContent = texto ?? '';
  return div.innerHTML;
}

// ---- Modal crear/editar ----

const modal = document.getElementById('modalUsuario');
const formUsuario = document.getElementById('formUsuario');

document.getElementById('btnNuevoUsuario').addEventListener('click', () => {
  formUsuario.reset();
  document.getElementById('usuarioId').value = '';
  document.getElementById('tituloModalUsuario').textContent = 'Nuevo usuario';
  document.getElementById('pistaPassword').textContent = '';
  document.getElementById('usuarioPassword').required = true;
  modal.classList.remove('oculto');
});

document.getElementById('btnCancelarUsuario').addEventListener('click', () => {
  modal.classList.add('oculto');
});

function abrirModalEdicion(id) {
  const usuario = usuariosCache.find(u => u.idUsuario === id);
  if (!usuario) return;

  document.getElementById('usuarioId').value = usuario.idUsuario;
  document.getElementById('usuarioNombre').value = usuario.nombre;
  document.getElementById('usuarioApellido').value = usuario.apellido;
  document.getElementById('usuarioEmail').value = usuario.email;
  document.getElementById('usuarioRol').value = usuario.rol;
  document.getElementById('usuarioPassword').value = '';
  document.getElementById('usuarioPassword').required = false;
  document.getElementById('pistaPassword').textContent = '(déjala vacía para no cambiarla)';
  document.getElementById('tituloModalUsuario').textContent = 'Editar usuario';
  modal.classList.remove('oculto');
}

formUsuario.addEventListener('submit', (e) => {
  e.preventDefault();

  const id = document.getElementById('usuarioId').value;
  const nombre = document.getElementById('usuarioNombre').value.trim();
  const apellido = document.getElementById('usuarioApellido').value.trim();
  const email = document.getElementById('usuarioEmail').value.trim();
  const password = document.getElementById('usuarioPassword').value;
  const rol = document.getElementById('usuarioRol').value;

  if (id) {
    // Nota: la modificación de contraseña no está incluida en GU-02 tal
    // como está definido en Core (Modificar no toca password). Si se
    // necesita, hay que agregar esa acción al UsuarioServicio.
    enviarAlHost({ accion: 'modificarUsuario', id: Number(id), nombre, apellido, email, rol });
  } else {
    enviarAlHost({ accion: 'crearUsuario', nombre, apellido, email, password, rol });
  }
});

function manejarCrearOModificarResultado(datos) {
  const mensajeEl = document.getElementById('adminMensaje');
  mensajeEl.textContent = datos.mensaje;
  mensajeEl.className = 'mensaje ' + (datos.ok ? 'exito' : 'error');

  if (datos.ok) {
    modal.classList.add('oculto');
    enviarAlHost({ accion: 'listarUsuarios' });
  }
}

function confirmarEliminacion(id) {
  const usuario = usuariosCache.find(u => u.idUsuario === id);
  const nombreCompleto = usuario ? `${usuario.nombre} ${usuario.apellido}` : 'este usuario';
  if (confirm(`¿Eliminar a ${nombreCompleto}? Esta acción no se puede deshacer.`)) {
    enviarAlHost({ accion: 'eliminarUsuario', id });
  }
}

function manejarEliminarResultado(datos) {
  const mensajeEl = document.getElementById('adminMensaje');
  mensajeEl.textContent = datos.mensaje;
  mensajeEl.className = 'mensaje ' + (datos.ok ? 'exito' : 'error');

  if (datos.ok) {
    enviarAlHost({ accion: 'listarUsuarios' });
  }
}
