// ARROW ARO RICE - Interactive Client Scripts

document.addEventListener('DOMContentLoaded', function () {
    // Intercept Inquiry and RFQ forms for smooth submission across cloud and static environments
    document.querySelectorAll('form[action*="SubmitInquiry"]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            var formData = new FormData(form);
            var name = formData.get('FullName') || formData.get('ContactPerson') || 'Customer';
            var company = formData.get('CompanyName') || '';
            var variety = formData.get('RiceVariety') || 'Basmati Rice';
            var qty = formData.get('QuantityMT') || '25';
            var port = formData.get('DestinationPort') || '';
            var phone = formData.get('Phone') || '';
            var email = formData.get('Email') || '';

            // Pre-filled WhatsApp message
            var waMsg = encodeURIComponent(
                "Hello ARROW ARO RICE Export Desk,\n\n" +
                "I would like to request an official FOB/CIF quotation:\n" +
                "• Buyer: " + name + (company ? " (" + company + ")" : "") + "\n" +
                "• Rice Variety: " + variety + "\n" +
                "• Quantity: " + qty + " MT\n" +
                (port ? "• Destination Port: " + port + "\n" : "") +
                (phone ? "• Contact: " + phone + "\n" : "") +
                "Please provide your latest price benchmark and packing specifications."
            );
            var waUrl = "https://wa.me/919876543210?text=" + waMsg;

            // Optional serverless post if running on Vercel
            try {
                fetch('/api/inquiry', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(Object.fromEntries(formData.entries()))
                }).catch(function () {});
            } catch (err) {}

            // Close existing Bootstrap modal if form was inside one
            var parentModal = form.closest('.modal');
            if (parentModal && window.bootstrap) {
                var modalInstance = bootstrap.Modal.getInstance(parentModal);
                if (modalInstance) {
                    modalInstance.hide();
                }
            }

            // Show celebratory confirmation modal
            var modalHtml = `
                <div class="modal fade show" id="inquirySuccessModal" tabindex="-1" style="display: block; background: rgba(0,0,0,0.65); z-index: 1060;" aria-modal="true" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content border-0 shadow-lg rounded-4 overflow-hidden text-center p-4">
                            <div class="mb-3">
                                <div class="d-inline-flex align-items-center justify-content-center bg-success bg-opacity-10 text-success rounded-circle" style="width: 76px; height: 76px;">
                                    <i class="fa-solid fa-circle-check fs-1"></i>
                                </div>
                            </div>
                            <h4 class="font-serif text-dark fw-bold mb-2">Quotation Request Logged!</h4>
                            <p class="text-muted small mb-4">
                                Thank you <strong>${name}</strong>. Your export inquiry for <strong>${qty} MT of ${variety}</strong> has been received by our international sales desk. Our commercial team will respond within 4 business hours.
                            </p>
                            <div class="d-flex flex-column gap-2">
                                <a href="${waUrl}" target="_blank" class="btn btn-success d-inline-flex align-items-center justify-content-center gap-2 py-2 fw-semibold shadow-sm rounded-3">
                                    <i class="fa-brands fa-whatsapp fs-5"></i> Connect on WhatsApp Instantly
                                </a>
                                <button type="button" class="btn btn-light border py-2 rounded-3 text-secondary" onclick="document.getElementById('inquirySuccessModal').remove();">
                                    Close
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            var existing = document.getElementById('inquirySuccessModal');
            if (existing) existing.remove();
            document.body.insertAdjacentHTML('beforeend', modalHtml);
            form.reset();
        });
    });
});
