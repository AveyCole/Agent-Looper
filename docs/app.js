(() => {
  'use strict';
  const motion = document.querySelector('#motion-toggle');
  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
  function syncMotionPreference() {
    if (reducedMotion.matches) {
      motion.disabled = true;
      motion.textContent = 'Motion reduced';
    } else {
      motion.disabled = false;
      const paused = document.body.classList.contains('motion-paused');
      motion.innerHTML = paused ? 'Resume motion <span aria-hidden="true">▶</span>' : 'Pause motion <span aria-hidden="true">Ⅱ</span>';
    }
  }
  motion.addEventListener('click', () => {
    const paused = document.body.classList.toggle('motion-paused');
    motion.setAttribute('aria-pressed', String(paused));
    syncMotionPreference();
  });
  syncMotionPreference();
  reducedMotion.addEventListener('change', syncMotionPreference);

  const screens = {
    loops: {file: 'screenshot.png?v=1.2.6', width: 1224, height: 881, caption: 'Actual Windows interface · Dual mode with separate layers', alt: 'Actual Agent Looper Windows interface showing two loopers, separate layers, waveform handles and recording controls'},
    effects: {file: 'effects.png?v=1.2.6', width: 1920, height: 1080, caption: 'Actual Windows interface · Dedicated effects page', alt: 'Actual Agent Looper Windows effects page showing A and B sends, effect routing, Freeze and granular controls'}
  };
  document.querySelectorAll('[data-screen]').forEach(button => button.addEventListener('click', () => {
    const selected = screens[button.dataset.screen];
    document.querySelectorAll('[data-screen]').forEach(item => {
      const active = item === button;
      item.classList.toggle('is-active', active);
      item.setAttribute('aria-pressed', String(active));
    });
    const image = document.querySelector('#app-screen');
    image.src = selected.file;
    image.width = selected.width;
    image.height = selected.height;
    image.alt = selected.alt;
    document.querySelector('#screen-full').href = selected.file;
    document.querySelector('#screen-body').classList.toggle('effects-view', button.dataset.screen === 'effects');
    const caption = document.querySelector('#screen-caption');
    caption.replaceChildren(Object.assign(document.createElement('span'), {className: 'status-dot'}), document.createTextNode(selected.caption));
    caption.firstElementChild.setAttribute('aria-hidden', 'true');
  }));

  const descriptions = {
    serial: 'Serial follows the visible tab order. Hold and drag the tabs in the app to change the order.',
    parallel: 'The original source feeds Grain, Delay and Reverb independently. Their outputs are combined before the looper’s filter.',
    split: 'Grain feeds Delay and Reverb in parallel. The two branches are combined before the looper’s filter.'
  };
  const labels = {
    serial: 'Serial routing: source through Grain, Delay and Reverb, then filter and output.',
    parallel: 'Parallel routing: source to Grain, Delay and Reverb independently; combined through filter to output.',
    split: 'Split routing: source through Grain, then separate Delay and Reverb branches; combined through filter to output.'
  };
  function node(x, y, name, grain = false, width = 140) {
    return `<rect class="node${grain ? ' grain-node' : ''}" x="${x}" y="${y}" width="${width}" height="54" rx="5"/><text x="${x + width / 2}" y="${y + 33}" text-anchor="middle">${name}</text>`;
  }
  function wire(path) { return `<path class="wire" d="${path}"/>`; }
  function renderRoute(mode) {
    let content;
    if (mode === 'serial') {
      content = '<text class="port" x="20" y="38">SOURCE</text><text class="port" x="660" y="224" text-anchor="end">FILTER → OUT</text>' +
        wire('M48 52V117H94 M234 117H270 M410 117H446 M586 117H632V197') +
        node(94, 90, 'GRAIN', true) + node(270, 90, 'DELAY') + node(446, 90, 'REVERB');
    } else if (mode === 'parallel') {
      content = '<text class="port" x="340" y="28" text-anchor="middle">SOURCE</text><text class="port" x="340" y="232" text-anchor="middle">FILTER → OUT</text>' +
        wire('M340 41V65 M140 65H540 M140 65V90 M340 65V90 M540 65V90 M140 144V185H540V144 M340 144V210') +
        node(70, 90, 'GRAIN', true) + node(270, 90, 'DELAY') + node(470, 90, 'REVERB');
    } else {
      content = '<text class="port" x="20" y="133">SOURCE</text><text class="port" x="660" y="133" text-anchor="end">FILTER → OUT</text>' +
        wire('M80 127H116 M256 127H306 M306 74V180 M306 74H350 M306 180H350 M490 74H534V180H490 M534 127H560') +
        node(116, 100, 'GRAIN', true) + node(350, 47, 'DELAY') + node(350, 153, 'REVERB');
    }
    const diagram = document.querySelector('#route-diagram');
    diagram.innerHTML = `<svg viewBox="0 0 680 258" aria-hidden="true">${content}</svg>`;
    diagram.setAttribute('aria-label', labels[mode]);
    document.querySelector('#route-description').textContent = descriptions[mode];
    document.querySelectorAll('[data-route]').forEach(button => {
      const active = button.dataset.route === mode;
      button.classList.toggle('is-active', active);
      button.setAttribute('aria-pressed', String(active));
    });
  }
  document.querySelectorAll('[data-route]').forEach(button => button.addEventListener('click', () => renderRoute(button.dataset.route)));
  renderRoute('serial');
})();
