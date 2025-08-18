// Minimal SideNav initialization without UI builder/demo extras

// Auto update layout helpers
(function () {
  if (window.layoutHelpers && typeof window.layoutHelpers.setAutoUpdate === 'function') {
    window.layoutHelpers.setAutoUpdate(true);
  }
})();

// Restore collapsed state from localStorage (only for vertical sidenav and non-small screens)
(function () {
  if (!window.layoutHelpers) return;
  var sidenavEl = document.getElementById('layout-sidenav');
  if (!sidenavEl || sidenavEl.classList.contains('sidenav-horizontal') || window.layoutHelpers.isSmallScreen()) {
    return;
  }
  try {
    var collapsed = localStorage.getItem('layoutCollapsed') === 'true';
    window.layoutHelpers.setCollapsed(collapsed, false);
  } catch (e) {
    // ignore
  }
})();

// jQuery-dependent initialization
$(function () {
  // Initialize sidenav
  $('#layout-sidenav').each(function () {
    // Orientation: vertical by default
    new SideNav(this, {
      orientation: $(this).hasClass('sidenav-horizontal') ? 'horizontal' : 'vertical'
    });
  });

  // Initialize sidenav togglers
  $('body').on('click', '.layout-sidenav-toggle', function (e) {
    e.preventDefault();
    if (!window.layoutHelpers) return;
    window.layoutHelpers.toggleCollapsed();
    if (!window.layoutHelpers.isSmallScreen()) {
      try {
        localStorage.setItem('layoutCollapsed', String(window.layoutHelpers.isCollapsed()));
      } catch (e) { /* ignore */ }
    }
  });

  // RTL dropdown alignment fix
  if ($('html').attr('dir') === 'rtl') {
    $('#layout-navbar .dropdown-menu').toggleClass('dropdown-menu-right');
  }
});



