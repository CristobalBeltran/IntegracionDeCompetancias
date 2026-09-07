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
    case 'marcarEntradaResultado': manejarMarcarResultado(datos, 'entrada'); break;
    case 'marcarSalidaResultado': manejarMarcarResultado(datos, 'salida'); break;
    case 'obtenerEstadoAsistenciaResultado': manejarEstadoAsistenciaResultado(datos); break;
    case 'obtenerReportesResultado': manejarObtenerReportesResultado(datos); break;
    case 'obtenerHistorialResultado': manejarObtenerHistorialResultado(datos); break;
    case 'modificarAsistenciaResultado': manejarModificarAsistenciaResultado(datos); break;
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
    document.getElementById('asistenciaMensaje').textContent = '';
    enviarAlHost({ accion: 'obtenerEstadoAsistencia', idUsuario: usuarioActual.idUsuario });
  }
}

document.getElementById('btnCerrarSesion').addEventListener('click', () => {
  usuarioActual = null;
  document.getElementById('formLogin').reset();
  document.getElementById('loginMensaje').textContent = '';
  actualizarBotonesAsistencia(false, false);
  actualizarBarraSuperior();
  mostrarPantalla('pantallaLogin');
});

// ============================================================
// PANEL EMPLEADO (CA-01)
// ============================================================

// Mientras se espera la respuesta del estado de hoy, se dejan deshabilitados
// para no permitir un doble marcado antes de saber si ya se marcó antes.
function actualizarBotonesAsistencia(entradaMarcada, salidaMarcada) {
  document.getElementById('btnMarcarEntrada').disabled = entradaMarcada;
  document.getElementById('btnMarcarSalida').disabled = salidaMarcada;
}

function manejarEstadoAsistenciaResultado(datos) {
  actualizarBotonesAsistencia(!!datos.entradaMarcada, !!datos.salidaMarcada);
}

document.getElementById('btnMarcarEntrada').addEventListener('click', () => {
  enviarAlHost({ accion: 'marcarEntrada', idUsuario: usuarioActual.idUsuario });
});

document.getElementById('btnMarcarSalida').addEventListener('click', () => {
  enviarAlHost({ accion: 'marcarSalida', idUsuario: usuarioActual.idUsuario });
});

function manejarMarcarResultado(datos, tipo) {
  mostrarMensajeAsistencia(datos);
  // Al marcar con éxito, se deshabilita de inmediato el botón usado para
  // evitar que se alcance a presionar dos veces (doble asistencia).
  if (datos.ok) {
    document.getElementById(tipo === 'entrada' ? 'btnMarcarEntrada' : 'btnMarcarSalida').disabled = true;
  }
}

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

// ============================================================
// PANEL ADMINISTRADOR — pestañas (Usuarios / Reportes)
// ============================================================

document.querySelectorAll('.tab').forEach(boton => {
  boton.addEventListener('click', () => mostrarSubPanelAdmin(boton.dataset.tab));
});

function mostrarSubPanelAdmin(nombre) {
  document.querySelectorAll('.tab').forEach(b => b.classList.remove('tab-activo'));
  document.querySelector(`.tab[data-tab="${nombre}"]`).classList.add('tab-activo');

  document.getElementById('subpanelUsuarios').classList.toggle('oculto', nombre !== 'usuarios');
  document.getElementById('subpanelReportes').classList.toggle('oculto', nombre !== 'reportes');
  document.getElementById('subpanelHistorial').classList.toggle('oculto', nombre !== 'historial');

  if (nombre === 'reportes') {
    enviarAlHost({ accion: 'obtenerReportes' });
  } else if (nombre === 'historial') {
    enviarAlHost({ accion: 'obtenerHistorial' });
  }
}

document.getElementById('btnActualizarReportes').addEventListener('click', () => {
  enviarAlHost({ accion: 'obtenerReportes' });
});

document.getElementById('btnActualizarHistorial').addEventListener('click', () => {
  enviarAlHost({ accion: 'obtenerHistorial' });
});

// ============================================================
// PANEL ADMINISTRADOR — Reportes (RE-01, RE-02, RE-03)
// ============================================================

function formatearFecha(fechaIso) {
  // El backend manda DateTime como "2026-08-19T00:00:00"; solo interesa la fecha.
  if (!fechaIso) return '';
  const [anio, mes, dia] = fechaIso.split('T')[0].split('-');
  return `${dia}-${mes}-${anio}`;
}

function formatearHora(hora) {
  // TimeSpan llega como "09:45:00"; se corta a "09:45" para que se lea mejor.
  return hora ? hora.substring(0, 5) : '-';
}

// Los reportes que trae el backend (más el historial completo), guardados
// tal cual llegan para poder reordenarlos en el cliente sin volver a pedirlos.
let reportesCache = { atrasos: [], salidasAnticipadas: [], inasistencias: [], historial: [] };

// Orden actual de cada tabla. Por defecto viene igual que lo que ya manda
// el backend (fecha, de más reciente a más antigua).
const ordenReportes = {
  atrasos: { campo: 'fecha', direccion: 'desc' },
  salidasAnticipadas: { campo: 'fecha', direccion: 'desc' },
  inasistencias: { campo: 'fecha', direccion: 'desc' },
  historial: { campo: 'fecha', direccion: 'desc' }
};

const CONFIG_REPORTES = {
  atrasos: { idCuerpo: 'cuerpoTablaAtrasos', idContador: 'contadorAtrasos', columnas: 4 },
  salidasAnticipadas: { idCuerpo: 'cuerpoTablaSalidasAnticipadas', idContador: 'contadorSalidasAnticipadas', columnas: 4 },
  inasistencias: { idCuerpo: 'cuerpoTablaInasistencias', idContador: 'contadorInasistencias', columnas: 3 },
  historial: { idCuerpo: 'cuerpoTablaHistorial', idContador: 'contadorHistorial', columnas: 5 }
};

function compararValores(a, b) {
  if (a == null && b == null) return 0;
  if (a == null) return -1;
  if (b == null) return 1;
  if (typeof a === 'string' && typeof b === 'string') return a.localeCompare(b, 'es');
  return a < b ? -1 : a > b ? 1 : 0;
}

function ordenarFilas(filas, orden) {
  const copia = filas.slice();
  copia.sort((f1, f2) => {
    const cmp = compararValores(f1[orden.campo], f2[orden.campo]);
    return orden.direccion === 'asc' ? cmp : -cmp;
  });
  return copia;
}

function llenarTablaReporte(idCuerpo, filas, totalColumnas, nombreReporte) {
  const cuerpo = document.getElementById(idCuerpo);
  cuerpo.innerHTML = '';

  if (!filas || filas.length === 0) {
    const fila = document.createElement('tr');
    fila.innerHTML = `<td colspan="${totalColumnas}" class="sin-datos">Sin registros</td>`;
    cuerpo.appendChild(fila);
    return;
  }

  filas.forEach(f => {
    const fila = document.createElement('tr');
    let celdaHora = '';
    if (nombreReporte === 'atrasos') {
      celdaHora = `<td><span class="etiqueta-hora etiqueta-atraso">${formatearHora(f.horaEntrada)}</span></td>`;
    } else if (nombreReporte === 'salidasAnticipadas') {
      celdaHora = `<td><span class="etiqueta-hora etiqueta-anticipada">${formatearHora(f.horaSalida)}</span></td>`;
    } else if (nombreReporte === 'historial') {
      celdaHora = `<td>${formatearHora(f.horaEntrada)}</td><td>${formatearHora(f.horaSalida)}</td>`;
    }
    fila.innerHTML = `
      <td>${escaparHtml(f.nombreCompleto)}</td>
      <td>${formatearFecha(f.fecha)}</td>
      ${celdaHora}
      <td class="acciones-fila">
        <button class="boton boton-secundario" data-accion="modificar-asistencia" data-id="${f.idAsistencia}">Modificar</button>
      </td>`;
    cuerpo.appendChild(fila);
  });

  cuerpo.querySelectorAll('[data-accion="modificar-asistencia"]').forEach(boton => {
    boton.addEventListener('click', () => abrirModalAsistencia(Number(boton.dataset.id)));
  });
}

function renderizarReporte(nombreReporte) {
  const cfg = CONFIG_REPORTES[nombreReporte];
  const filas = reportesCache[nombreReporte] || [];
  document.getElementById(cfg.idContador).textContent = filas.length;

  const filasOrdenadas = ordenarFilas(filas, ordenReportes[nombreReporte]);
  llenarTablaReporte(cfg.idCuerpo, filasOrdenadas, cfg.columnas, nombreReporte);
}

function manejarObtenerReportesResultado(datos) {
  reportesCache.atrasos = datos.atrasos || [];
  reportesCache.salidasAnticipadas = datos.salidasAnticipadas || [];
  reportesCache.inasistencias = datos.inasistencias || [];
  renderizarReporte('atrasos');
  renderizarReporte('salidasAnticipadas');
  renderizarReporte('inasistencias');
}

function manejarObtenerHistorialResultado(datos) {
  reportesCache.historial = datos.historial || [];
  renderizarReporte('historial');
}

// ---- Encabezados clicables para ordenar cada tabla ----

function actualizarFlechasOrden(tabla, campoActivo, direccion) {
  tabla.querySelectorAll('th[data-orden]').forEach(th => {
    const flecha = th.querySelector('.flecha-orden');
    const esActiva = th.dataset.orden === campoActivo;
    th.classList.toggle('col-orden-activa', esActiva);
    flecha.textContent = esActiva ? (direccion === 'asc' ? '▲' : '▼') : '';
  });
}

document.querySelectorAll('.tabla-ordenable').forEach(tabla => {
  const nombreReporte = tabla.dataset.reporte;
  tabla.querySelectorAll('th[data-orden]').forEach(th => {
    th.addEventListener('click', () => {
      const campo = th.dataset.orden;
      const orden = ordenReportes[nombreReporte];

      if (orden.campo === campo) {
        orden.direccion = orden.direccion === 'asc' ? 'desc' : 'asc';
      } else {
        orden.campo = campo;
        orden.direccion = 'asc';
      }

      actualizarFlechasOrden(tabla, campo, orden.direccion);
      renderizarReporte(nombreReporte);
    });
  });
});

// ---- Modal: modificar un registro de asistencia ----

const modalAsistencia = document.getElementById('modalAsistencia');
const formAsistencia = document.getElementById('formAsistencia');

function buscarFilaAsistencia(idAsistencia) {
  return reportesCache.atrasos.find(f => f.idAsistencia === idAsistencia)
    || reportesCache.salidasAnticipadas.find(f => f.idAsistencia === idAsistencia)
    || reportesCache.inasistencias.find(f => f.idAsistencia === idAsistencia)
    || reportesCache.historial.find(f => f.idAsistencia === idAsistencia);
}

function abrirModalAsistencia(idAsistencia) {
  const fila = buscarFilaAsistencia(idAsistencia);
  if (!fila) return;

  document.getElementById('asistenciaIdRegistro').value = idAsistencia;
  document.getElementById('asistenciaResumen').textContent =
    `${fila.nombreCompleto} — ${formatearFecha(fila.fecha)}`;
  document.getElementById('asistenciaHoraEntrada').value = fila.horaEntrada ? formatearHora(fila.horaEntrada) : '';
  document.getElementById('asistenciaHoraSalida').value = fila.horaSalida ? formatearHora(fila.horaSalida) : '';
  document.getElementById('asistenciaModalMensaje').textContent = '';
  modalAsistencia.classList.remove('oculto');
}

document.getElementById('btnCancelarAsistencia').addEventListener('click', () => {
  modalAsistencia.classList.add('oculto');
});

formAsistencia.addEventListener('submit', (e) => {
  e.preventDefault();
  const idAsistencia = Number(document.getElementById('asistenciaIdRegistro').value);
  const horaEntrada = document.getElementById('asistenciaHoraEntrada').value || null;
  const horaSalida = document.getElementById('asistenciaHoraSalida').value || null;

  const fila = buscarFilaAsistencia(idAsistencia);
  const quien = fila ? `${fila.nombreCompleto} (${formatearFecha(fila.fecha)})` : 'este registro';
  const confirmado = confirm(
    `¿Estás seguro de modificar la asistencia de ${quien}?\n` +
    `Entrada: ${horaEntrada || '(sin hora)'} — Salida: ${horaSalida || '(sin hora)'}`
  );
  if (!confirmado) return;

  enviarAlHost({ accion: 'modificarAsistencia', idAsistencia, horaEntrada, horaSalida });
});

function manejarModificarAsistenciaResultado(datos) {
  const mensajeEl = document.getElementById('asistenciaModalMensaje');
  mensajeEl.textContent = datos.mensaje;
  mensajeEl.className = 'mensaje ' + (datos.ok ? 'exito' : 'error');

  if (datos.ok) {
    modalAsistencia.classList.add('oculto');
    // Refresca la pestaña que esté visible (Reportes o Historial), según
    // desde dónde se haya abierto el modal de modificación.
    const tabActivo = document.querySelector('.tab.tab-activo')?.dataset.tab;
    enviarAlHost({ accion: tabActivo === 'historial' ? 'obtenerHistorial' : 'obtenerReportes' });
  }
}
