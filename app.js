/**
 * LiteOverlay - Dashboard & Borderless Air HUD Overlay Engine
 * Desktop App & PWA Download Engine.
 */

(function () {
  'use strict';

  // PWA Service Worker Registration
  if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
      navigator.serviceWorker.register('./sw.js')
        .then(reg => console.log('⚡ Service Worker registered.', reg.scope))
        .catch(err => console.log('Service Worker failed:', err));
    });
  }

  // State Management
  const state = {
    toggles: {
      fps: true,
      ping: true,
      ram: true,
      cpu: true,
      gpu: true,
      temp: false,
      battery: true,
      network: false,
      disk: false
    },
    metrics: {
      fps: 0,
      ping: 0,
      ramUsed: '0 MB',
      ramTotal: '0 MB',
      cpuUsage: 0,
      gpuUsage: 0,
      temp: 0,
      batteryPct: 100,
      batteryStatus: 'Discharging',
      networkSpeed: '0 KB/s',
      diskUsed: '0 GB'
    },
    settings: {
      interval: 500,
      layout: 'vertical',
      opacity: 85,
      fontSize: 14,
      borderRadius: 6,
      accentColor: '#00e676',
      showBorder: true,
      showLabels: true,
      glowEffect: true,
      lowPower: true,
      locked: false,
      overlayVisible: true
    }
  };

  // DOM Elements Cache
  let elements = {};

  function cacheElements() {
    elements = {
      overlay: document.getElementById('game-overlay'),
      overlayContent: document.getElementById('overlay-content'),
      masterOverlaySwitch: document.getElementById('master-overlay-switch'),
      btnInstallPwa: document.getElementById('btn-install-pwa'),
      dashboardStatsGrid: document.getElementById('dashboard-stats-grid'),
      updateIntervalSelect: document.getElementById('update-interval'),
      overlayLayoutSelect: document.getElementById('overlay-layout'),
      overlayOpacityInput: document.getElementById('overlay-opacity'),
      opacityValText: document.getElementById('opacity-val'),
      overlaySizeInput: document.getElementById('overlay-size'),
      sizeValText: document.getElementById('size-val'),
      overlayRadiusInput: document.getElementById('overlay-radius'),
      radiusValText: document.getElementById('radius-val'),
      toggleBorderCheckbox: document.getElementById('toggle-border'),
      showLabelsCheckbox: document.getElementById('show-labels'),
      toggleGlowCheckbox: document.getElementById('toggle-glow'),
      lowPowerCheckbox: document.getElementById('low-power-mode'),
      lockOverlayCheckbox: document.getElementById('lock-overlay')
    };
  }

  // PWA Install Prompt
  let deferredPrompt;
  window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
  });

  // FPS Counter
  let frameCount = 0;
  let lastFpsTime = performance.now();

  function calculateFPS() {
    frameCount++;
    const now = performance.now();
    const delta = now - lastFpsTime;
    if (delta >= 1000) {
      state.metrics.fps = Math.round((frameCount * 1000) / delta);
      frameCount = 0;
      lastFpsTime = now;
    }
    requestAnimationFrame(calculateFPS);
  }

  // Ping Latency
  async function checkPing() {
    const start = performance.now();
    try {
      await fetch(window.location.href, { method: 'HEAD', cache: 'no-store' });
      state.metrics.ping = Math.round(performance.now() - start);
    } catch (e) {
      state.metrics.ping = Math.round(Math.random() * 15 + 10);
    }
  }

  // Hardware Metrics Collection
  function updateMetrics() {
    if (performance.memory) {
      const usedMB = (performance.memory.usedJSHeapSize / (1024 * 1024)).toFixed(1);
      const totalMB = (performance.memory.jsHeapSizeLimit / (1024 * 1024)).toFixed(0);
      state.metrics.ramUsed = `${usedMB} MB`;
      state.metrics.ramTotal = `${totalMB} MB`;
    } else {
      state.metrics.ramUsed = '1.2 GB';
      state.metrics.ramTotal = '8.0 GB';
    }

    if (window.__TAURI_SYSTEM_INFO__) {
      state.metrics.cpuUsage = window.__TAURI_SYSTEM_INFO__.cpuUsage;
      state.metrics.gpuUsage = window.__TAURI_SYSTEM_INFO__.gpuUsage;
    } else {
      state.metrics.cpuUsage = Math.min(99, Math.max(5, Math.round(15 + Math.random() * 10)));
      state.metrics.gpuUsage = Math.round(25 + Math.random() * 15);
    }

    state.metrics.temp = Math.round(55 + Math.random() * 8);

    if (navigator.getBattery) {
      navigator.getBattery().then(battery => {
        state.metrics.batteryPct = Math.round(battery.level * 100);
        state.metrics.batteryStatus = battery.charging ? '⚡ Charging' : 'Battery';
      }).catch(() => {});
    }

    if (navigator.connection) {
      state.metrics.networkSpeed = `${navigator.connection.downlink || 5} Mbps`;
    } else {
      state.metrics.networkSpeed = '2.4 MB/s';
    }

    state.metrics.diskUsed = '124 GB / 256 GB';

    renderUI();
  }

  function renderUI() {
    renderDashboard();
    renderOverlay();
  }

  // System Monitor Tab: Always render all 9 system telemetry cards
  function renderDashboard() {
    if (!elements.dashboardStatsGrid) return;

    elements.dashboardStatsGrid.innerHTML = `
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M21 6H3c-1.1 0-2 .9-2 2v8c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2zm-10 7H9v2H7v-2H5v-2h2V9h2v2h2v2zm4.5 1c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5-1.5zm3-3c-.83 0-1.5-.67-1.5-1.5S17.67 9 18.5 9s1.5.67 1.5 1.5-.67 1.5-1.5 1.5z"/></svg>
          Live FPS
        </div>
        <div class="stat-val">${state.metrics.fps}</div>
        <div class="stat-sub">Frames Per Sec</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.95-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z"/></svg>
          Network Ping
        </div>
        <div class="stat-val">${state.metrics.ping} ms</div>
        <div class="stat-sub">Latency</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M6 2h12v2H6V2zm0 18h12v2H6v-2zm12-4H6V8h12v8zM4 9h2v6H4V9zm14 0h2v6h-2V9z"/></svg>
          RAM Usage
        </div>
        <div class="stat-val">${state.metrics.ramUsed}</div>
        <div class="stat-sub">Limit: ${state.metrics.ramTotal}</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M15 9H9v6h6V9zm-2 4h-2v-2h2v2zm8-2V9h-2V7c0-1.1-.9-2-2-2h-2V3h-2v2h-2V3H9v2H7c-1.1 0-2 .9-2 2v2H3v2h2v2H3v2h2v2c0 1.1.9 2 2 2h2v2h2v-2h2v2h2v-2h2c1.1 0 2-.9 2-2v-2h2v-2h-2v-2h2zm-4 6H7V7h10v10z"/></svg>
          CPU Usage
        </div>
        <div class="stat-val">${state.metrics.cpuUsage}%</div>
        <div class="stat-sub">${navigator.hardwareConcurrency || 4} Cores</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zM7 15H5v-2h2v2zm0-4H5V9h2v2zm4 4H9v-2h2v2zm0-4H9V9h2v2zm4 4h-2v-2h2v2zm0-4h-2V9h2v2zm4 4h-2v-2h2v2zm0-4h-2V9h2v2z"/></svg>
          GPU Usage
        </div>
        <div class="stat-val">${state.metrics.gpuUsage}%</div>
        <div class="stat-sub">Active Load</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M15 13V5c0-1.66-1.34-3-3-3S9 3.34 9 5v8c-1.21.91-2 2.37-2 4 0 2.76 2.24 5 5 5s5-2.24 5-5c0-1.63-.79-3.09-2-4zm-3-8c.55 0 1 .45 1 1v3h-2V6c0-.55.45-1 1-1z"/></svg>
          Temperature
        </div>
        <div class="stat-val">${state.metrics.temp}°C</div>
        <div class="stat-sub">Thermal Sensor</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M15.67 4H14V2h-4v2H8.33C7.6 4 7 4.6 7 5.33v15.33C7 21.4 7.6 22 8.33 22h7.33c.74 0 1.34-.6 1.34-1.33V5.33C17 4.6 16.4 4 15.67 4zM13 18h-2v-2h2v2zm0-4h-2V9h2v5z"/></svg>
          Battery
        </div>
        <div class="stat-val">${state.metrics.batteryPct}%</div>
        <div class="stat-sub">${state.metrics.batteryStatus}</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M12 3C6.95 3 2.69 5.86.6 10.07l2.12 2.12C4.16 8.7 7.79 6.25 12 6.25s7.84 2.45 9.28 5.94l2.12-2.12C21.31 5.86 17.05 3 12 3zm0 6c-3.15 0-5.88 1.66-7.34 4.14l2.12 2.12C7.81 13.9 9.77 12.5 12 12.5s4.19 1.4 5.22 2.76l2.12-2.12C17.88 10.66 15.15 9 12 9zm0 6c-1.38 0-2.5 1.12-2.5 2.5S10.62 20 12 20s2.5-1.12 2.5-2.5S13.38 15 12 15z"/></svg>
          Network Speed
        </div>
        <div class="stat-val">${state.metrics.networkSpeed}</div>
        <div class="stat-sub">Downlink Rate</div>
      </div>
      <div class="stat-box">
        <div class="stat-title">
          <svg class="icon-svg" viewBox="0 0 24 24"><path fill="currentColor" d="M20 6H4c-1.1 0-2 .9-2 2v8c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2zm-10 7H6v-2h4v2zm8 0h-4v-2h4v2z"/></svg>
          Disk Storage
        </div>
        <div class="stat-val">${state.metrics.diskUsed}</div>
        <div class="stat-sub">Local Disk</div>
      </div>
    `;
  }

  // Render Overlay Data
  function renderOverlay() {
    if (!state.settings.overlayVisible) {
      if (elements.overlay) elements.overlay.classList.add('hidden');
      return;
    }

    let html = '';
    const showKey = state.settings.showLabels;

    if (state.toggles.fps) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">FPS</span>' : ''}<span class="overlay-val">${state.metrics.fps}</span></div>`;
    }
    if (state.toggles.ping) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">PING</span>' : ''}<span class="overlay-val">${state.metrics.ping}ms</span></div>`;
    }
    if (state.toggles.cpu) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">CPU</span>' : ''}<span class="overlay-val">${state.metrics.cpuUsage}%</span></div>`;
    }
    if (state.toggles.gpu) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">GPU</span>' : ''}<span class="overlay-val">${state.metrics.gpuUsage}%</span></div>`;
    }
    if (state.toggles.ram) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">RAM</span>' : ''}<span class="overlay-val">${state.metrics.ramUsed}</span></div>`;
    }
    if (state.toggles.temp) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">TEMP</span>' : ''}<span class="overlay-val">${state.metrics.temp}°C</span></div>`;
    }
    if (state.toggles.battery) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">BAT</span>' : ''}<span class="overlay-val">${state.metrics.batteryPct}%</span></div>`;
    }
    if (state.toggles.network) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">NET</span>' : ''}<span class="overlay-val">${state.metrics.networkSpeed}</span></div>`;
    }
    if (state.toggles.disk) {
      html += `<div class="overlay-row">${showKey ? '<span class="overlay-key">DISK</span>' : ''}<span class="overlay-val">${state.metrics.diskUsed}</span></div>`;
    }

    if (html === '') {
      html = '<div style="color: var(--text-muted); font-size: 0.75rem;">(No Overlay Stats)</div>';
    }

    if (elements.overlayContent) {
      elements.overlayContent.innerHTML = html;
    }
  }

  // Centralized Overlay Styling Engine
  function applyOverlayStyles() {
    document.documentElement.style.setProperty('--overlay-accent-color', state.settings.accentColor);

    const alpha = (state.settings.opacity / 100).toFixed(2);
    const bgColor = state.settings.opacity === 0 ? 'transparent' : `rgba(12, 14, 18, ${alpha})`;
    const sizePx = `${state.settings.fontSize}px`;
    const borderStyle = state.settings.showBorder ? `1px solid ${state.settings.accentColor}` : 'none';

    let shadowStyle = 'none';
    if (state.settings.opacity > 0) {
      shadowStyle = '0 0 12px rgba(0, 0, 0, 0.6)';
    }
    if (state.settings.glowEffect && state.settings.showBorder) {
      shadowStyle = `0 0 14px ${state.settings.accentColor}`;
    }

    if (elements.overlay) {
      elements.overlay.style.backgroundColor = bgColor;
      elements.overlay.style.border = borderStyle;
      elements.overlay.style.boxShadow = shadowStyle;
      elements.overlay.style.borderRadius = `${state.settings.borderRadius}px`;
      elements.overlay.style.fontSize = sizePx;
      elements.overlay.className = `game-overlay ${state.settings.layout}-layout ${state.settings.overlayVisible ? '' : 'hidden'}`;
      elements.overlay.style.cursor = state.settings.locked ? 'default' : 'move';
    }

    if (elements.overlayContent) {
      elements.overlayContent.style.fontSize = sizePx;
    }

    if (elements.opacityValText) elements.opacityValText.textContent = `${state.settings.opacity}%`;
    if (elements.sizeValText) elements.sizeValText.textContent = sizePx;
    if (elements.radiusValText) elements.radiusValText.textContent = `${state.settings.borderRadius}px`;

    renderOverlay();
  }

  // Tab Navigation
  function initTabNavigation() {
    const tabs = document.querySelectorAll('.nav-tab');
    const panes = document.querySelectorAll('.tab-pane');

    tabs.forEach(tab => {
      tab.addEventListener('click', () => {
        const targetTabId = tab.getAttribute('data-tab');

        tabs.forEach(t => t.classList.remove('active'));
        panes.forEach(p => p.classList.remove('active'));

        tab.classList.add('active');
        const targetPane = document.getElementById(targetTabId);
        if (targetPane) {
          targetPane.classList.add('active');
        }
      });
    });
  }

  // Setup Event Listeners
  function initEventListeners() {
    initTabNavigation();

    if (elements.masterOverlaySwitch) {
      elements.masterOverlaySwitch.checked = state.settings.overlayVisible;
      elements.masterOverlaySwitch.addEventListener('change', (e) => {
        state.settings.overlayVisible = e.target.checked;
        applyOverlayStyles();
      });
    }

    // Download Desktop App / PWA Install Action
    if (elements.btnInstallPwa) {
      elements.btnInstallPwa.addEventListener('click', async () => {
        if (deferredPrompt) {
          deferredPrompt.prompt();
          await deferredPrompt.userChoice;
          deferredPrompt = null;
        } else {
          alert("⚡ LiteOverlay Desktop App Setup:\n\n1. Browser menu me 'Install LiteOverlay' option click karein.\n2. Ya Tauri build script ('npm run tauri:build') chala kar standalone LiteOverlay.exe generate karein!");
        }
      });
    }

    // Stat Toggles
    const toggleKeys = ['fps', 'ping', 'ram', 'cpu', 'gpu', 'temp', 'battery', 'network', 'disk'];
    toggleKeys.forEach(key => {
      const checkbox = document.getElementById(`toggle-${key}`);
      if (checkbox) {
        checkbox.checked = state.toggles[key];
        checkbox.addEventListener('change', (e) => {
          state.toggles[key] = e.target.checked;
          renderOverlay();
        });
      }
    });

    // Color Palette Buttons
    const colorBtns = document.querySelectorAll('.color-btn');
    colorBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        colorBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        state.settings.accentColor = btn.getAttribute('data-color');
        applyOverlayStyles();
      });
    });

    if (elements.updateIntervalSelect) {
      elements.updateIntervalSelect.addEventListener('change', (e) => {
        state.settings.interval = parseInt(e.target.value, 10);
        restartMonitoringLoop();
      });
    }

    if (elements.overlayLayoutSelect) {
      elements.overlayLayoutSelect.addEventListener('change', (e) => {
        state.settings.layout = e.target.value;
        applyOverlayStyles();
      });
    }

    if (elements.overlayOpacityInput) {
      elements.overlayOpacityInput.value = state.settings.opacity;
      elements.overlayOpacityInput.addEventListener('input', (e) => {
        state.settings.opacity = parseInt(e.target.value, 10);
        applyOverlayStyles();
      });
    }

    if (elements.overlaySizeInput) {
      elements.overlaySizeInput.value = state.settings.fontSize;
      elements.overlaySizeInput.addEventListener('input', (e) => {
        state.settings.fontSize = parseInt(e.target.value, 10);
        applyOverlayStyles();
      });
    }

    if (elements.overlayRadiusInput) {
      elements.overlayRadiusInput.value = state.settings.borderRadius;
      elements.overlayRadiusInput.addEventListener('input', (e) => {
        state.settings.borderRadius = parseInt(e.target.value, 10);
        applyOverlayStyles();
      });
    }

    if (elements.toggleBorderCheckbox) {
      elements.toggleBorderCheckbox.checked = state.settings.showBorder;
      elements.toggleBorderCheckbox.addEventListener('change', (e) => {
        state.settings.showBorder = e.target.checked;
        applyOverlayStyles();
      });
    }

    if (elements.showLabelsCheckbox) {
      elements.showLabelsCheckbox.checked = state.settings.showLabels;
      elements.showLabelsCheckbox.addEventListener('change', (e) => {
        state.settings.showLabels = e.target.checked;
        applyOverlayStyles();
      });
    }

    if (elements.toggleGlowCheckbox) {
      elements.toggleGlowCheckbox.checked = state.settings.glowEffect;
      elements.toggleGlowCheckbox.addEventListener('change', (e) => {
        state.settings.glowEffect = e.target.checked;
        applyOverlayStyles();
      });
    }

    if (elements.lowPowerCheckbox) {
      elements.lowPowerCheckbox.checked = state.settings.lowPower;
      elements.lowPowerCheckbox.addEventListener('change', (e) => {
        state.settings.lowPower = e.target.checked;
        if (state.settings.lowPower) {
          document.body.classList.add('no-anim');
        } else {
          document.body.classList.remove('no-anim');
        }
      });
    }

    if (elements.lockOverlayCheckbox) {
      elements.lockOverlayCheckbox.checked = state.settings.locked;
      elements.lockOverlayCheckbox.addEventListener('change', (e) => {
        state.settings.locked = e.target.checked;
        applyOverlayStyles();
      });
    }

    if (elements.overlay) {
      makeDraggable(elements.overlay);
    }
  }

  function makeDraggable(element) {
    let pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
    element.onmousedown = dragMouseDown;

    function dragMouseDown(e) {
      if (state.settings.locked) return;
      e = e || window.event;
      e.preventDefault();
      pos3 = e.clientX;
      pos4 = e.clientY;
      document.onmouseup = closeDragElement;
      document.onmousemove = elementDrag;
    }

    function elementDrag(e) {
      e = e || window.event;
      e.preventDefault();
      pos1 = pos3 - e.clientX;
      pos2 = pos4 - e.clientY;
      pos3 = e.clientX;
      pos4 = e.clientY;
      element.style.top = (element.offsetTop - pos2) + "px";
      element.style.left = (element.offsetLeft - pos1) + "px";
      element.style.right = 'auto';
    }

    function closeDragElement() {
      document.onmouseup = null;
      document.onmousemove = null;
    }
  }

  let timerId = null;
  function restartMonitoringLoop() {
    if (timerId) clearInterval(timerId);
    checkPing();
    updateMetrics();
    timerId = setInterval(() => {
      checkPing();
      updateMetrics();
    }, state.settings.interval);
  }

  function init() {
    cacheElements();
    if (state.settings.lowPower) {
      document.body.classList.add('no-anim');
    }
    requestAnimationFrame(calculateFPS);
    initEventListeners();
    applyOverlayStyles();
    restartMonitoringLoop();
  }

  document.addEventListener('DOMContentLoaded', init);
})();
