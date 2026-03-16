document.addEventListener('DOMContentLoaded', function () {

    // ЛОГІКА ПОШУКУ ТА ФІЛЬТРАЦІЇ
  
    const searchInput = document.getElementById('liveSearch');
    const capacityFilter = document.getElementById('capacityFilter');
    const capacityValueDisplay = document.getElementById('capacityValue');

    function filterItems() {
        const term = searchInput ? searchInput.value.toLowerCase() : "";

        let minCapacity = 0;
        if (capacityFilter) {
            minCapacity = parseInt(capacityFilter.value) || 0;

            if (capacityValueDisplay) {
                capacityValueDisplay.textContent = minCapacity === 0 ? "Будь-яка" : "Від " + minCapacity + " місць";
            }
        }

        document.querySelectorAll('.search-item').forEach(item => {
            const searchableText = (item.getAttribute('data-search') || "").toLowerCase();
            const textMatch = searchableText.includes(term);

            const itemCapacity = parseInt(item.getAttribute('data-capacity') || "0");
            const capacityMatch = itemCapacity >= minCapacity;

            if (textMatch && capacityMatch) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', filterItems);
    }

    if (capacityFilter) {
        capacityFilter.addEventListener('input', filterItems);
        capacityFilter.addEventListener('change', filterItems);
    }

  
    //  АНІМАЦІЇ 3D-КАРТОК
    
    document.querySelectorAll('.holographic-card').forEach(card => {
        const inner = card.querySelector('.holographic-card__inner');

       
        if (!inner) return;

        card.addEventListener('mousemove', e => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            const px = (x / rect.width) * 100;
            const py = (y / rect.height) * 100;

            const tiltX = (py - 50) / 3;
            const tiltY = (50 - px) / 3;

            inner.style.transform = `rotateX(${tiltX}deg) rotateY(${tiltY}deg)`;
        });

        card.addEventListener('mouseleave', () => {
            inner.style.transform = `rotateX(0deg) rotateY(0deg)`;
        });
    });

});