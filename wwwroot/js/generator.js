// Angular CRUD Generator - JavaScript Functions
var currentDbType = 'SqlServer';

// Helper: get current table name from input
function getTableName() {
    return (document.getElementById('tableNameInput') || {}).value?.trim() || '';
}

document.addEventListener('DOMContentLoaded', function () {
    checkDbStatus();
    toggleCrudOptions();
    toggleDataSource();

    document.getElementById('genForm').addEventListener('submit', function (e) {
        e.preventDefault();
        handleGenerateClick();
        return false;
    });

    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// --- Handle Generate Button Click ---
function handleGenerateClick() {
    var sqlChecked = document.getElementById('sqlSource').checked;
    var apiChecked = document.getElementById('apiSource').checked;
    var jsonChecked = document.getElementById('jsonSource').checked;

    // ถ้าเลือก API หรือ JSON
    if (apiChecked || jsonChecked) {
        if (!window.parsedFields || window.parsedFields.length === 0) {
            showToast('กรุณา ' + (apiChecked ? 'Fetch Schema จาก API' : 'Parse JSON') + ' ก่อนกด Generate Code', 'warning');
            return;
        }

        generateFromParsedFields();
        return;
    }

    // SQL mode
    if (sqlChecked) {
        var tableName = getTableName();
        if (!tableName) {
            showToast('กรุณาเลือก Table Name', 'warning');
            document.getElementById('tableNameInput')?.focus();
            return;
        }
        generateFromSql();
    }
}

// --- Generate Code from SQL (No Refresh) ---
async function generateFromSql() {
    // แสดง loading
    showLoadingState();

    try {
        // รวบรวมข้อมูลจาก form
        var formData = new FormData(document.getElementById('genForm'));

        // แปลง FormData เป็น URLSearchParams สำหรับ POST
        var params = new URLSearchParams();
        for (var pair of formData.entries()) {
            params.append(pair[0], pair[1]);
        }

        // เรียก backend
        var response = await fetch('/Generator/GenerateFromSql', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: params.toString()
        });

        var data = await response.json();

        if (data.success) {
            displayGeneratedCode(data);
            showToast('Generate code สำเร็จ!', 'success');
        } else {
            hideLoadingState();
            showToast('Error: ' + (data.errorMessage || 'Failed to generate code'), 'error');
        }
    } catch (error) {
        hideLoadingState();
        showToast('Failed to generate code: ' + error.message, 'error');
    }
}

// --- Show/Hide Loading State ---
function showLoadingState() {
    var emptyState = document.getElementById('emptyState');
    var loadingState = document.getElementById('loadingState');

    if (emptyState) emptyState.style.display = 'none';
    if (loadingState) loadingState.style.display = 'block';
}

function hideLoadingState() {
    var loadingState = document.getElementById('loadingState');
    if (loadingState) loadingState.style.display = 'none';
}

// --- Logic: Toggle CRUD Options based on Mode ---
function toggleCrudOptions() {
    var modeReport = document.getElementById('modeReport');
    var modeCrud = document.getElementById('modeCrud');
    var crudCard = document.getElementById('crudOptionsCard');

    if (modeReport && modeReport.checked) {
        crudCard.style.display = 'none';
        document.getElementById('IsGetById').checked = false;
        document.getElementById('IsPost').checked = false;
        document.getElementById('IsUpdate').checked = false;
        document.getElementById('IsDelete').checked = false;
    } else {
        crudCard.style.display = 'block';
    }
}

function checkDbStatus() {
    var dot = document.getElementById('dbStatusDot');
    var text = document.getElementById('dbStatusText');
    dot.className = 'status-dot';
    text.innerText = 'Checking...';

    fetch('/Generator/CheckDbConnection')
        .then(r => r.json())
        .then(d => {
            if (d.success) {
                currentDbType = d.dbType;
                toggleAs400UI(d.dbType === 'AS400');
                
                dot.className = 'status-dot status-online';
                const dbIcon = d.dbType === 'MySQL' ? '🐬' : d.dbType === 'PostgreSQL' ? '🐘' : d.dbType === 'AS400' ? '📠' : '💾';
                text.innerText = `${dbIcon} ${d.dbType}: ${d.databaseName}`;
                // โหลดรายการ table เมื่อ DB เชื่อมต่อสำเร็จ
                loadTableList();
            } else {
                dot.className = 'status-dot status-offline';
                text.innerText = 'DB Disconnected';
                console.error('DB Check Failed:', d.message);
                if (d.message && d.message !== "Connection failed - Unable to open connection") {
                     showToast('DB Error: ' + d.message, 'error');
                }
            }
        })
        .catch(e => {
            dot.className = 'status-dot status-offline';
            text.innerText = 'Error';
            console.error('CheckDbConnection Exception:', e);
        });
}

// --- Load Table List ---
async function loadTableList() {
    const status = document.getElementById('loadStatus');
    const input = document.getElementById('tableNameInput');
    const datalist = document.getElementById('tableDataList');
    const refreshBtn = document.querySelector('.btn-refresh');

    try {
        if (refreshBtn) refreshBtn.disabled = true;
        if (status) {
            status.textContent = 'Loading tables...';
            status.className = 'text-info small';
        }

        const response = await fetch('/Generator/GetTables');
        const data = await response.json();

        if (data.success) {
            // Inject <option> values into <datalist> — browser shows autocomplete/dropdown
            datalist.innerHTML = data.tables
                .map(t => `<option value="${t}">`)
                .join('');

            updateTableCount(data.tables.length, data.tables.length);

            if (data.tables.length > 0) {
                showToast(`Loaded ${data.tables.length} tables`, 'success');
                if (status) status.textContent = '';
            } else {
                showToast('No tables found', 'warning');
                if (status) status.textContent = 'No tables found';
            }
        } else {
            console.error('Failed to load tables:', data.message);
            if (status) {
                status.textContent = 'Failed to load tables';
                status.className = 'text-danger small';
            }
        }
    } catch (error) {
        console.error('Failed to load table list:', error);
        if (status) {
            status.textContent = 'Error loading tables';
            status.className = 'text-danger small';
        }
        showToast('Failed to load table list: ' + error.message, 'error');
    } finally {
        if (refreshBtn) refreshBtn.disabled = false;
    }
}

// Replaced by Semantic UI
function filterTables() { }
function clearTableSearch() { }

// --- Clear Table and Field Lists ---
function clearTableAndFieldLists() {
    // Clear native input + datalist
    const input = document.getElementById('tableNameInput');
    const datalist = document.getElementById('tableDataList');
    if (input) input.value = '';
    if (datalist) datalist.innerHTML = '';
    
    // Clear field containers
    const sqlFieldContainer = document.getElementById('sqlFieldContainer');
    const apiFieldContainer = document.getElementById('apiFieldContainer');
    const jsonFieldContainer = document.getElementById('jsonFieldContainer');
    
    if (sqlFieldContainer) {
        sqlFieldContainer.innerHTML = '<div class="h-100 d-flex align-items-center justify-content-center" style="min-height: 120px;"><small class="text-muted fst-italic">Press \'Load\' to see columns...</small></div>';
    }
    
    if (apiFieldContainer) {
        apiFieldContainer.innerHTML = '<div class="h-100 d-flex align-items-center justify-content-center" style="min-height: 120px;"><small class="text-muted fst-italic">Please fetch schema to load fields from API</small></div>';
    }
    
    if (jsonFieldContainer) {
        jsonFieldContainer.innerHTML = '<div class="h-100 d-flex align-items-center justify-content-center" style="min-height: 120px;"><small class="text-muted fst-italic">Please parse JSON to load fields</small></div>';
    }
    
    // Clear status
    const status = document.getElementById('loadStatus');
    if (status) {
        status.textContent = '';
        status.className = '';
    }
}

// --- Update Table Count ---
function updateTableCount(filtered, total) {
    const status = document.getElementById('loadStatus');
    if (filtered === total) {
        status.textContent = `📊 ${total} table${total !== 1 ? 's' : ''} available`;
        status.className = 'text-muted small';
    } else {
        status.textContent = `🔍 Found ${filtered} of ${total} table${total !== 1 ? 's' : ''}`;
        status.className = 'text-info small';
    }
}


// --- Logic: Load Columns (AJAX) ---
function loadColumns() {
    var tableName = getTableName();
    // ปุ่มและ status แตกต่างกันระหว่าง SQL / AS400
    var isAs400 = currentDbType === 'AS400';
    var btn    = document.getElementById(isAs400 ? 'btnLoadAs400' : 'btnLoad');
    var status = document.getElementById(isAs400 ? 'loadStatusAs400' : 'loadStatus');
    var container = document.getElementById('sqlFieldContainer');
    if (!tableName) {
        showToast('กรุณาระบุ Table Name', 'warning'); return;
    }

    btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
    status.innerText = '';

    fetch('/Generator/GetColumns?tableName=' + tableName)
        .then(r => r.json())
        .then(data => {
            btn.disabled = false;
            btn.innerHTML = '<i class="fa-solid fa-sync"></i> Load Columns Data';
            container.innerHTML = '';

            if (data.success) {
                status.className = 'text-success small'; status.innerText = 'Found ' + data.fields.length + ' columns.';

                // สร้างตารางสำหรับแสดง Fields พร้อม PK badge
                var table = `
                            <table class="table table-sm mb-0">
                                <thead>
                                    <tr>
                                        <th style="width: 40px;"><i class="fas fa-check text-success" title="Include"></i></th>
                                        <th>Field Name</th>
                                        <th>Type</th>
                                    </tr>
                                </thead>
                                <tbody>`;

                data.fields.forEach((f, idx) => {
                    var isPK = f.isPrimaryKey;
                    var pkBadge = isPK ? '<i class="fas fa-key text-warning ms-1" title="Primary Key (Required)"></i><span class="badge bg-warning text-dark ms-1" style="font-size: 0.65rem;">PK</span>' : '';
                    var disabled = isPK ? 'disabled' : '';
                    var rowClass = isPK ? 'table-warning' : '';
                    var hiddenInput = isPK ? `<input type="hidden" name="selectedFields" value="${f.fieldName}">` : '';

                    table += `
                                <tr class="${rowClass}">
                                    <td class="text-center">
                                        <input class="form-check-input field-checkbox" type="checkbox" 
                                               name="selectedFields" value="${f.fieldName}" checked ${disabled}>
                                        ${hiddenInput}
                                    </td>
                                    <td>
                                        <span class="fw-bold text-dark small">${f.fieldName}</span>
                                        ${pkBadge}
                                    </td>
                                    <td><span class="text-muted small">${f.tsType}</span></td>
                                </tr>`;
                });

                table += `</tbody></table>`;
                container.innerHTML = table;

                // Reset Select All
                document.getElementById('selectAllFields').checked = true;
            } else {
                status.className = 'text-danger small'; status.innerText = 'Error: ' + data.message;
                container.innerHTML = '<div class="h-100 d-flex align-items-center justify-content-center" style="min-height: 120px;"><small class="text-muted fst-italic">Press \'Load\' to see columns...</small></div>';
            }
        })
        .catch(e => {
            btn.disabled = false; btn.innerHTML = 'Load';
            container.innerHTML = '<div class="h-100 d-flex align-items-center justify-content-center" style="min-height: 120px;"><small class="text-muted fst-italic">Press \'Load\' to see columns...</small></div>';
        });
}

// --- Logic: Toggle Select All (Fields) ---
function toggleAllFields(source) {
    var checkboxes = document.querySelectorAll('.field-checkbox:not([disabled])');
    checkboxes.forEach(cb => cb.checked = source.checked);
}

// --- Logic: Toggle Select All (CRUD) ---
function toggleAllCrud(source) {
    var checkboxes = document.querySelectorAll('.crud-checkbox');
    checkboxes.forEach(cb => cb.checked = source.checked);
}

// --- Logic: Auto-check GET BY ID when POST/UPDATE/DELETE selected ---
function updateGetByIdRequirement() {
    var isPost = document.getElementById('IsPost').checked;
    var isUpdate = document.getElementById('IsUpdate').checked;
    var isDelete = document.getElementById('IsDelete').checked;
    var getByIdCheckbox = document.getElementById('IsGetById');

    // ถ้าเลือก POST, UPDATE, หรือ DELETE → บังคับให้ GET BY ID ติ๊กด้วย
    if (isPost || isUpdate || isDelete) {
        getByIdCheckbox.checked = true;
    }
}

// --- Logic: Copy to Clipboard ---
function copyText(elementId, isTextContent = false) {
    var element = document.getElementById(elementId);
    var text = isTextContent ? element.innerText : (element.value || element.textContent);
    navigator.clipboard.writeText(text).then(
        () => showToast('Copied to clipboard!', 'success'),
        () => showToast('Failed to copy', 'error')
    );
}

// --- Download All as ZIP ---
async function downloadAllAsZip() {
    try {
        if (typeof JSZip === 'undefined') {
            showToast('JSZip library not loaded!', 'error');
            return;
        }

        const zip = new JSZip();

        const entityName = getTableName() || 'component';
        const selector = entityName.toLowerCase().replace(/[^a-z0-9]/g, '-');

        const htmlContent = document.getElementById('codeHtml')?.value;
        if (htmlContent && htmlContent.trim()) {
            zip.file(`${selector}.component.html`, htmlContent);
        }

        const tsContent = document.getElementById('codeTs')?.value;
        if (tsContent && tsContent.trim()) {
            zip.file(`${selector}.component.ts`, tsContent);
        }

        const svcContent = document.getElementById('codeSvc')?.value;
        if (svcContent && svcContent.trim()) {
            zip.file(`${selector}.service.ts`, svcContent);
        }

        const interfaceContent = document.getElementById('codeInterface')?.value;
        if (interfaceContent && interfaceContent.trim()) {
            zip.file(`${selector}.interface.ts`, interfaceContent);
        }

        const cssContent = document.getElementById('codeCss')?.value;
        if (cssContent && cssContent.trim()) {
            zip.file(`${selector}.component.css`, cssContent);
        }

        const readme = `# ${entityName} Component

Generated by Angular CRUD Generator
Date: ${new Date().toLocaleString()}

## Files Included:
- ${selector}.component.html
- ${selector}.component.ts
- ${selector}.service.ts
${interfaceContent ? `- ${selector}.interface.ts\n` : ''}${cssContent ? `- ${selector}.component.css\n` : ''}
## Installation:
1. Copy all files to your Angular project
2. Import the component in your module
3. Update the API endpoint in the service file

Enjoy! 🚀
`;
        zip.file('README.md', readme);

        // Generate ZIP file
        showToast('Generating ZIP file...', 'info');
        const blob = await zip.generateAsync({ type: 'blob' });

        // Download using FileSaver.js
        saveAs(blob, `${selector}-component.zip`);

        showToast('ZIP file downloaded successfully!', 'success');
    } catch (error) {
        console.error('Download error:', error);
        showToast('Failed to create ZIP file: ' + error.message, 'error');
    }
}

// --- Data Source Toggle ---
function toggleDataSource() {
    var sqlChecked = document.getElementById('sqlSource').checked;
    var apiChecked = document.getElementById('apiSource').checked;
    var jsonChecked = document.getElementById('jsonSource').checked;

    // แสดง/ซ่อน Data Source sections
    document.getElementById('sqlSection').style.display = sqlChecked ? 'block' : 'none';
    document.getElementById('apiSection').style.display = apiChecked ? 'block' : 'none';
    document.getElementById('jsonSection').style.display = jsonChecked ? 'block' : 'none';

    // แสดง/ซ่อน Field containers แยกกันชัดเจน
    document.getElementById('sqlFieldContainer').style.display = sqlChecked ? 'block' : 'none';
    document.getElementById('apiFieldContainer').style.display = apiChecked ? 'block' : 'none';
    document.getElementById('jsonFieldContainer').style.display = jsonChecked ? 'block' : 'none';

    // จัดการ required attribute สำหรับ select dropdown
    var $tableSelect = $('#semanticTableDropdown');
    // (select element is part of form — required handled by Fomantic UI validation)

    var apiBaseUrlSection = document.getElementById('apiBaseUrlSection');
    if (apiChecked) {
        apiBaseUrlSection.style.display = 'none';
    } else {
        apiBaseUrlSection.style.display = 'block';
    }
}

// --- API Headers Management ---
let headerCount = 0;
function addApiHeader() {
    headerCount++;
    var container = document.getElementById('apiHeadersContainer');
    var headerHtml = `
                <div class="input-group input-group-sm mb-2" id="header${headerCount}">
                    <input type="text" class="form-control" placeholder="Key" id="headerKey${headerCount}">
                    <input type="text" class="form-control" placeholder="Value" id="headerValue${headerCount}">
                    <button type="button" class="btn btn-outline-danger" onclick="removeApiHeader(${headerCount})">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `;
    container.insertAdjacentHTML('beforeend', headerHtml);
}

function removeApiHeader(id) {
    document.getElementById('header' + id).remove();
}

function getApiHeaders() {
    var headers = {};
    var container = document.getElementById('apiHeadersContainer');
    var headerDivs = container.querySelectorAll('[id^="header"]');

    headerDivs.forEach(div => {
        var id = div.id.replace('header', '');
        var key = document.getElementById('headerKey' + id)?.value;
        var value = document.getElementById('headerValue' + id)?.value;
        if (key && value) {
            headers[key] = value;
        }
    });

    return headers;
}

// --- Fetch API Schema ---
async function fetchApiSchema() {
    var apiUrl = document.getElementById('apiUrl').value;
    var httpMethod = document.getElementById('httpMethod').value;
    var entityName = document.getElementById('entityNameApi').value || 'Item';
    var headers = getApiHeaders();

    if (!apiUrl) {
        showToast('Please enter API URL', 'warning');
        return;
    }

    try {
        var response = await fetch('/Generator/ParseApiSchema', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sourceType: 1,
                apiUrl: apiUrl,
                httpMethod: httpMethod,
                headers: headers,
                entityName: entityName
            })
        });

        var data = await response.json();

        if (data.success) {
            displayParsedFields(data.fields, entityName, 'apiFieldContainer');
            // For API/JSON mode, store entity name as hidden value for form submit
            window.parsedEntityName = entityName;
        } else {
            showToast('Error: ' + data.errorMessage, 'danger');
        }
    } catch (error) {
        showToast('Failed to fetch API schema: ' + error.message, 'danger');
    }
}

// --- Parse JSON Schema ---
async function parseJsonSchema() {
    var jsonContent = document.getElementById('jsonSample').value;
    var entityName = document.getElementById('entityNameJson').value || 'Item';

    if (!jsonContent.trim()) {
        showToast('Please paste JSON sample', 'warning');
        return;
    }

    try {
        var response = await fetch('/Generator/ParseJsonSchema', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sourceType: 3,
                jsonContent: jsonContent,
                entityName: entityName
            })
        });

        var data = await response.json();

        if (data.success) {
            displayParsedFields(data.fields, entityName, 'jsonFieldContainer');
            window.parsedEntityName = entityName;
        } else {
            showToast('Error: ' + data.errorMessage, 'danger');
        }
    } catch (error) {
        showToast('Failed to parse JSON: ' + error.message, 'danger');
    }
}

// --- Display Parsed Fields ---
function displayParsedFields(fields, entityName, containerId) {
    var container = document.getElementById(containerId);
    var status = document.getElementById('loadStatus');

    status.className = 'text-success small';
    status.innerText = `Detected ${fields.length} fields from ${entityName}`;

    // Store parsed fields globally for code generation
    window.parsedFields = fields;
    window.parsedEntityName = entityName;

    var table = `
                <table class="table table-sm mb-0">
                    <thead>
                        <tr>
                            <th style="width: 40px;"><i class="fas fa-check text-success"></i></th>
                            <th>Field Name</th>
                            <th>Type</th>
                        </tr>
                    </thead>
                    <tbody>`;

    fields.forEach(f => {
        var isPk = f.isPrimaryKey || false;
        var rowClass = isPk ? 'table-warning' : '';
        var pkBadge = isPk ? '<i class="fas fa-key text-warning ms-1"></i> <span class="badge bg-warning text-dark" style="font-size: 0.65rem;">PK</span>' : '';
        var disabled = isPk ? 'disabled' : '';
        var checked = 'checked';

        table += `
                    <tr class="${rowClass}">
                        <td class="text-center">
                            <input class="form-check-input field-checkbox" type="checkbox" 
                                   name="selectedFields" value="${f.fieldName}" ${checked} ${disabled}>
                            ${isPk ? `<input type="hidden" name="selectedFields" value="${f.fieldName}">` : ''}
                        </td>
                        <td>
                            <span class="fw-bold text-dark small">${f.fieldName}</span>
                            ${pkBadge}
                        </td>
                        <td>
                            <span class="badge bg-info small">${f.tsType}</span>
                            <span class="badge bg-secondary small">${getControlTypeName(f.uiControl)}</span>
                        </td>
                    </tr>`;
    });

    table += `</tbody></table>`;
    container.innerHTML = table;

    // Update select all checkbox
    document.getElementById('selectAllFields').checked = true;
}

function getControlTypeName(controlType) {
    var types = ['Text', 'Number', 'DatePicker', 'Checkbox', 'TextArea'];
    return types[controlType] || 'Text';
}

// --- Generate Code from Parsed Fields ---
async function generateFromParsedFields() {
    if (!window.parsedFields || !window.parsedFields.length) {
        showToast('No fields to generate', 'warning');
        return;
    }

    // แสดง loading
    showLoadingState();

    try {

        var selectedCheckboxes = document.querySelectorAll('.field-checkbox:checked:not([disabled])');
        var selectedFields = Array.from(selectedCheckboxes).map(cb => cb.value);

        var pkHidden = document.querySelector('input[type="hidden"][name="selectedFields"]');
        if (pkHidden) {
            selectedFields.unshift(pkHidden.value);
        }

        var request = {
            entityName: window.parsedEntityName || 'Item',
            fields: window.parsedFields,
            selectedFields: selectedFields,
            apiBaseUrl: document.getElementById('apiBaseUrl')?.value || 'http://localhost:3000/api',
            generationMode: document.querySelector('input[name="generationMode"]:checked')?.value || 'CRUD',
            layoutType: document.querySelector('input[name="layoutType"]:checked')?.value || 'TableView',
            cssFramework: document.querySelector('input[name="cssFramework"]:checked')?.value || 'BasicCSS',
            separateInterface: document.getElementById('separateInterface')?.checked || false,
            isGet: true,
            isGetById: document.getElementById('IsGetById')?.checked || false,
            isPost: document.getElementById('IsPost')?.checked || false,
            isUpdate: document.getElementById('IsUpdate')?.checked || false,
            isDelete: document.getElementById('IsDelete')?.checked || false
        };

        var response = await fetch('/Generator/GenerateFromParsedFields', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });

        var data = await response.json();

        if (data.success) {
            displayGeneratedCode(data);
            showToast('Generate code สำเร็จ!', 'success');
        } else {
            hideLoadingState();
            showToast('Error: ' + data.errorMessage, 'danger');
        }
    } catch (error) {
        hideLoadingState();
        showToast('Failed to generate code: ' + error.message, 'danger');
    }
}

// --- HTML Escape Function for Prism.js ---
function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function displayGeneratedCode(data) {
    // ซ่อน empty state และแสดง result card
    var parentCol = document.querySelector('.col-md-8');

    // รับค่า layout และ css framework
    var layoutType = data.layoutType || 'TableView';
    var cssFramework = data.cssFramework || 'Bootstrap';

    // Generate Dependency Check UI
    var dependencyAlert = '';
    if (data.isGet) {
        dependencyAlert = `
            <div class="alert alert-warning border-start border-4 border-warning shadow-sm mb-3">
                <div class="d-flex align-items-center mb-2">
                    <i class="fas fa-terminal me-2 fs-5"></i>
                    <strong class="text-dark">Required Dependencies</strong>
                </div>
                <p class="small mb-2">To use the <strong>Excel & PDF Export</strong> features, please run this command in your project terminal:</p>
                <div class="bg-dark text-white p-2 rounded d-flex justify-content-between align-items-center font-monospace" style="font-size: 0.85rem;">
                    <span id="npmCommand">npm install xlsx@^0.18.5 jspdf@^2.5.2 jspdf-autotable@^3.8.3</span>
                    <button type="button" class="btn btn-sm btn-outline-light py-0 px-2" onclick="copyText('npmCommand', true)" title="Copy Command">
                        <i class="fa-regular fa-copy" style="font-size: 0.7rem;"></i>
                    </button>
                </div>
            </div>`;
    }

    // Build the main UI with CLI command outside and Result card
    parentCol.innerHTML = `
                <label class="fw-bold mb-1">Create Component</label>
                <div class="bg-secondary text-white p-2 rounded mb-3 d-flex justify-content-between align-items-center font-monospace">
                    <span id="cliCommand">ng generate component <strong>${data.selector || 'component-name'}</strong> --standalone</span>
                    <button type="button" class="btn btn-sm btn-dark" onclick="copyText('cliCommand', true)"><i class="fa-regular fa-copy"></i></button>
                </div>

                <!-- Setup Guide Action -->
                <div class="alert alert-primary border-0 shadow-sm d-flex justify-content-between align-items-center py-2 mb-3">
                    <div class="d-flex align-items-center">
                        <div class="bg-primary text-white rounded-circle p-2 me-3" style="width: 35px; height: 35px; display: flex; align-items: center; justify-content: center;">
                            <i class="fas fa-magic"></i>
                        </div>
                        <div>
                            <h6 class="mb-0 fw-bold">Ready to implement?</h6>
                            <small class="text-muted">Follow our step-by-step guide to integrate this code.</small>
                        </div>
                    </div>
                    <button type="button" class="btn btn-primary btn-sm px-3 shadow-sm fw-bold" data-bs-toggle="modal" data-bs-target="#guideModal">
                        <i class="fas fa-book-open me-1"></i> Open Setup Guide
                    </button>
                </div>

                <div class="card card-result shadow-sm h-75" style="margin-bottom: 10px;" id="resultCard">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <span>Result for ${data.entityName || ''}</span>

                        <div class="d-flex align-items-center">
                            <div class="me-3"> 
                                <span class="badge bg-info">
                                    <i class="fas fa-${layoutType === 'TableView' ? 'table' : 'th-large'}"></i>
                                    ${layoutType}
                                </span>
                                <span class="badge bg-${cssFramework === 'BasicCSS' ? 'warning' : cssFramework === 'Bootstrap' ? 'primary' : 'success'}">
                                    ${cssFramework}
                                    ${cssFramework === 'BasicCSS' ? '<i class="fas fa-file-code ms-1"></i>' : ''}
                                </span>
                            </div>
                            
                            <button type="button" class="btn btn-warning btn-sm text-dark fw-bold px-3 shadow-sm" onclick="downloadAllAsZip()">
                                <i class="fas fa-download me-1"></i> Download All
                            </button>
                        </div>
                    </div>
                    <div class="card-body p-0 result-area">
                        <ul class="nav nav-tabs bg-light px-2 pt-2" role="tablist">
                            <li class="nav-item"><a class="nav-link active" data-bs-toggle="tab" href="#tab-html">HTML</a></li>
                            <li class="nav-item"><a class="nav-link" data-bs-toggle="tab" href="#tab-ts">TypeScript</a></li>
                            <li class="nav-item"><a class="nav-link" data-bs-toggle="tab" href="#tab-svc">Service</a></li>
                            <li class="nav-item" id="tab-interface-nav" style="${data.interfaceCode ? '' : 'display:none'}">
                                <a class="nav-link" data-bs-toggle="tab" href="#tab-interface">Interface</a>
                            </li>
                            <li class="nav-item" id="tab-css-nav" style="${data.css ? '' : 'display:none'}">
                                <a class="nav-link" data-bs-toggle="tab" href="#tab-css">CSS</a>
                            </li>
                        </ul>
                        <div class="tab-content h-75">
                            <div class="tab-pane fade show active h-100" id="tab-html">
                                <button type="button" class="btn btn-secondary btn-sm copy-btn" onclick="copyText('codeHtml')"><i class="fa-regular fa-copy"></i> Copy HTML</button>
                                <pre class="h-100 m-0" style="overflow: auto;"><code id="codeHtml" class="language-markup">${escapeHtml(data.html)}</code></pre>
                            </div>
                            <div class="tab-pane fade h-100" id="tab-ts">
                                <button type="button" class="btn btn-secondary btn-sm copy-btn" onclick="copyText('codeTs')"><i class="fa-regular fa-copy"></i> Copy TS</button>
                                <pre class="h-100 m-0" style="overflow: auto;"><code id="codeTs" class="language-typescript">${escapeHtml(data.ts)}</code></pre>
                            </div>
                            <div class="tab-pane fade h-100" id="tab-svc">
                                <button type="button" class="btn btn-secondary btn-sm copy-btn" onclick="copyText('codeSvc')"><i class="fa-regular fa-copy"></i> Copy Service</button>
                                <pre class="h-100 m-0" style="overflow: auto;"><code id="codeSvc" class="language-typescript">${escapeHtml(data.service)}</code></pre>
                            </div>
                            <div class="tab-pane fade h-100" id="tab-interface">
                                <button type="button" class="btn btn-secondary btn-sm copy-btn" onclick="copyText('codeInterface')"><i class="fa-regular fa-copy"></i> Copy Interface</button>
                                <pre class="h-100 m-0" style="overflow: auto;"><code id="codeInterface" class="language-typescript">${escapeHtml(data.interfaceCode)}</code></pre>
                            </div>
                            <div class="tab-pane fade h-100" id="tab-css">
                                <button type="button" class="btn btn-secondary btn-sm copy-btn" onclick="copyText('codeCss')"><i class="fa-regular fa-copy"></i> Copy CSS</button>
                                <pre class="h-100 m-0" style="overflow: auto;"><code id="codeCss" class="language-css">${escapeHtml(data.css)}</code></pre>
                            </div>
                        </div>
                    </div>
                </div>
            `;

    // Update Modal Content
    const guideModalBody = document.getElementById('guideModalBody');
    if (guideModalBody) {
        guideModalBody.innerHTML = `
            <div class="accordion shadow-sm" id="guideAccordionModal">
                <!-- Step 1: Prerequisites -->
                <div class="accordion-item border-0 mb-2 rounded shadow-sm">
                    <h2 class="accordion-header">
                        <button class="accordion-button bg-white text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#mStep1">
                            <i class="fas fa-terminal me-2 text-primary"></i> 1. Prepare Component & Dependencies
                        </button>
                    </h2>
                    <div id="mStep1" class="accordion-collapse collapse show" data-bs-parent="#guideAccordionModal">
                        <div class="accordion-body bg-white pt-0">
                            <label class="small text-muted mb-1 mt-2 d-block">สร้าง Component ด้วยคำสั่ง:</label>
                            <div class="bg-dark text-white p-2 rounded mb-3 d-flex justify-content-between align-items-center font-monospace" style="font-size: 0.85rem;">
                                <span id="cliCommandModal">ng generate component <strong>${data.selector || 'component-name'}</strong> --standalone</span>
                                <button type="button" class="btn btn-sm btn-outline-light py-0 px-2" onclick="copyText('cliCommandModal', true)"><i class="fa-regular fa-copy"></i></button>
                            </div>
                            
                            ${dependencyAlert ? `
                            <div class="alert alert-info border-0 small mb-0">
                                <p class="mb-1 fw-bold text-dark"><i class="fas fa-info-circle me-1 text-info"></i> Excel & PDF Export:</p>
                                <p class="mb-2 text-dark">หากคุณเปิดใช้งานฟีเจอร์ Export กรุณาติดตั้ง Library เพิ่มเติม:</p>
                                <div class="bg-dark text-white p-2 rounded d-flex justify-content-between align-items-center font-monospace mb-0" style="font-size: 0.75rem;">
                                    <span id="npmCommandModal">npm install xlsx@^0.18.5 jspdf@^2.5.2 jspdf-autotable@^3.8.3</span>
                                    <button type="button" class="btn btn-sm btn-outline-light py-0 px-1" onclick="copyText('npmCommandModal', true)"><i class="fa-regular fa-copy"></i></button>
                                </div>
                            </div>
                            ` : ''}
                        </div>
                    </div>
                </div>

                <!-- Step 2: Routing Configuration -->
                <div class="accordion-item border-0 mb-2 rounded shadow-sm">
                    <h2 class="accordion-header">
                        <button class="accordion-button collapsed bg-white text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#mStep2">
                            <i class="fas fa-route me-2 text-info"></i> 2. Register Routing
                        </button>
                    </h2>
                    <div id="mStep2" class="accordion-collapse collapse" data-bs-parent="#guideAccordionModal">
                        <div class="accordion-body bg-white pt-0">
                            <label class="small text-muted mb-2 mt-2 d-block">Add this route to <strong>src/app/app.routes.ts</strong>:</label>
                            <div class="d-flex justify-content-end mb-2">
                                <button type="button" class="btn btn-sm btn-outline-primary py-0" onclick="copyText('codeRoutingModal')">
                                    <i class="fa-regular fa-copy me-1"></i> Copy Route
                                </button>
                            </div>
                            <div class="rounded-3 overflow-hidden border">
                                <pre class="m-0" style="background: #1e1e1e; font-size: 0.85rem;"><code id="codeRoutingModal" class="language-typescript">import { Routes } from '@angular/router';
import { ${data.entityName}Component } from './${(data.entityName || '').toLowerCase()}/${(data.entityName || '').toLowerCase()}.component';

export const routes: Routes = [
  { 
    path: '${data.selector || ''}', 
    component: ${data.entityName}Component,
    title: '${data.entityName} Management' 
  }
];</code></pre>
                            </div>
                            <p class="small text-muted mt-2 mb-0">URL: <code>http://localhost:4200/${data.selector || ''}</code></p>
                        </div>
                    </div>
                </div>

                <!-- Step 3: Global Setup -->
                <div class="accordion-item border-0 mb-2 rounded shadow-sm">
                    <h2 class="accordion-header">
                        <button class="accordion-button collapsed bg-white text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#mStep3">
                            <i class="fas fa-tools me-2 text-success"></i> 3. Global Project Setup
                        </button>
                    </h2>
                    <div id="mStep3" class="accordion-collapse collapse" data-bs-parent="#guideAccordionModal">
                        <div class="accordion-body bg-white pt-0 text-dark">
                            <p class="small mb-2 mt-2">Configure <strong>src/app/app.config.ts</strong> for Standalone apps:</p>
                            <div class="rounded-3 overflow-hidden border mb-3">
                                <pre class="m-0" style="background: #1e1e1e; font-size: 0.85rem;"><code class="language-typescript">import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideAnimationsAsync(),
  ]
};</code></pre>
                            </div>
                            <div class="card border-info bg-light mb-0">
                                <div class="card-body py-2">
                                    <small class="text-info fw-bold"><i class="fas fa-exclamation-triangle me-1"></i> CORS Warning:</small>
                                    <p class="small mb-0 text-dark">อย่าลืมอนุญาต URL <code>http://localhost:4200</code> ที่ API Backend</p>
                                </div>
                            </div>
                            ${data.cssFramework === 'AngularMaterial' ? `
                            <div class="mt-2 p-2 border rounded bg-light">
                                <small class="fw-bold d-block mb-1 text-primary">Add to styles.css:</small>
                                <code class="small">@import "@angular/material/prebuilt-themes/indigo-pink.css";</code>
                            </div>` : ''}
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Highlight code with Prism
    setTimeout(() => {
        Prism.highlightAll();

        // Scroll to result
        var resultCard = document.getElementById('resultCard');
        if (resultCard) {
            resultCard.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }, 100);
}

function showToast(message, type = 'success') {
    const toastContainer = document.getElementById('toastContainer');
    let bgClass, iconClass, title;
    switch (type) {
        case 'success':
            bgClass = 'text-bg-success';
            iconClass = 'fa-check-circle';
            title = 'Success';
            break;
        case 'error':
            bgClass = 'text-bg-danger';
            iconClass = 'fa-exclamation-circle';
            title = 'Error';
            break;
        case 'warning':
            bgClass = 'text-bg-warning';
            iconClass = 'fa-exclamation-triangle';
            title = 'Warning';
            break;
        default: // info
            bgClass = 'text-bg-primary';
            iconClass = 'fa-info-circle';
            title = 'Info';
    }

    const toastId = 'toast_' + Math.floor(Math.random() * 1000000);

    const toastHtml = `
                <div id="${toastId}" class="toast align-items-center ${bgClass} border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="d-flex">
                        <div class="toast-body">
                            <i class="fas ${iconClass} me-2"></i> <strong>${title}:</strong> ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// --- Database Configuration Modal Functions ---
function openDbConfigModal() {
    const modal = new bootstrap.Modal(document.getElementById('dbConfigModal'));
    updateDbFields(); // Set initial hints
    loadSavedConfigurations(); // Load saved configurations
    modal.show();
}

function updateDbFields() {
    const dbType = document.querySelector('input[name="modalDbType"]:checked').value;
    const portField = document.getElementById('modalPortField');
    const databaseField = document.getElementById('modalDatabaseField');
    const databaseLabel = document.getElementById('modalDatabaseLabel');
    const usernameField = document.getElementById('modalUsernameField');
    const passwordField = document.getElementById('modalPasswordField');
    const windowsAuthField = document.getElementById('modalWindowsAuthField');
    const connStringHint = document.getElementById('connStringHint');
    const portInput = document.getElementById('modalPort');
    const databaseInput = document.getElementById('modalDatabase');

    // Show/hide fields based on DB type
    if (dbType === 'SqlServer') {
        portField.style.display = 'none';
        databaseField.style.display = 'block';
        databaseLabel.textContent = 'Database';
        windowsAuthField.style.display = 'block';
        toggleAuthFields(); // Update username/password visibility
        portInput.value = '';
        databaseInput.placeholder = 'mydatabase';
        connStringHint.textContent = 'ตัวอย่าง: Server=localhost\\SQLEXPRESS;Database=mydb;Trusted_Connection=True;TrustServerCertificate=True';
    } else if (dbType === 'MySQL') {
        portField.style.display = 'block';
        databaseField.style.display = 'block';
        databaseLabel.textContent = 'Database';
        usernameField.style.display = 'block';
        passwordField.style.display = 'block';
        windowsAuthField.style.display = 'none';
        portInput.value = '3306';
        databaseInput.placeholder = 'mydatabase';
        connStringHint.textContent = 'ตัวอย่าง: Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=password;';
    } else if (dbType === 'PostgreSQL') {
        portField.style.display = 'block';
        databaseField.style.display = 'block';
        databaseLabel.textContent = 'Database';
        usernameField.style.display = 'block';
        passwordField.style.display = 'block';
        windowsAuthField.style.display = 'none';
        portInput.value = '5432';
        databaseInput.placeholder = 'mydatabase';
        connStringHint.textContent = 'ตัวอย่าง: Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=password;';
    } else if (dbType === 'AS400') {
        // AS400/IBM i ไม่มีแนวคิด Database แต่ใช้ Library
        portField.style.display = 'none';
        databaseField.style.display = 'block'; // แสดง field แต่เปลี่ยน label
        databaseLabel.textContent = 'Default Library (Optional)';
        databaseInput.placeholder = 'MYLIB';
        usernameField.style.display = 'block';
        passwordField.style.display = 'block';
        windowsAuthField.style.display = 'none';
        portInput.value = '';
        connStringHint.textContent = 'ตัวอย่าง: Provider=IBMDA400.DataSource.1;Data Source=192.168.1.100;User Id=user;Password=pass;Default Collection=MYLIB';
    }
}

function toggleAuthFields() {
    const useWindowsAuth = document.getElementById('modalWindowsAuth').checked;
    const usernameField = document.getElementById('modalUsernameField');
    const passwordField = document.getElementById('modalPasswordField');

    usernameField.style.display = useWindowsAuth ? 'none' : 'block';
    passwordField.style.display = useWindowsAuth ? 'none' : 'block';
}

function buildConnectionString() {
    const dbType = document.querySelector('input[name="modalDbType"]:checked').value;
    const server = document.getElementById('modalServer').value.trim();
    const database = document.getElementById('modalDatabase').value.trim();
    const port = document.getElementById('modalPort').value.trim();
    const username = document.getElementById('modalUsername').value.trim();
    const password = document.getElementById('modalPassword').value.trim();
    const useWindowsAuth = document.getElementById('modalWindowsAuth').checked;

    let connString = '';

    if (dbType === 'SqlServer') {
        if (useWindowsAuth) {
            connString = `Server=${server || 'localhost'};Database=${database || 'mydb'};Trusted_Connection=True;TrustServerCertificate=True`;
        } else {
            connString = `Server=${server || 'localhost'};Database=${database || 'mydb'};User Id=${username};Password=${password};TrustServerCertificate=True`;
        }
    } else if (dbType === 'MySQL') {
        connString = `Server=${server || 'localhost'};Port=${port || '3306'};Database=${database || 'mydb'};Uid=${username || 'root'};Pwd=${password};`;
    } else if (dbType === 'PostgreSQL') {
        connString = `Host=${server || 'localhost'};Port=${port || '5432'};Database=${database || 'mydb'};Username=${username || 'postgres'};Password=${password};`;
    } else if (dbType === 'AS400') {
        // AS400/IBM i - Database field คือ Default Library (Optional)
        connString = `Provider=IBMDA400.DataSource.1;Data Source=${server || 'localhost'};User Id=${username || 'user'};Password=${password};`;
        if (database) {
            connString += `Default Collection=${database};`;
        }
    }

    document.getElementById('modalConnString').value = connString;
}

async function testConnection() {
    const dbType = document.querySelector('input[name="modalDbType"]:checked').value;
    const connString = document.getElementById('modalConnString').value.trim();

    if (!connString) {
        showToast('กรุณากรอก Connection String', 'warning');
        return;
    }

    const testResult = document.getElementById('testResult');
    testResult.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Testing...';
    testResult.className = 'small text-info';

    try {
        const response = await fetch('/Generator/TestConnection', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                dbType: dbType,
                connectionString: connString
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();

        if (data.success) {
            testResult.innerHTML = `<i class="fas fa-check-circle"></i> Connected to: ${data.databaseName}`;
            testResult.className = 'small text-success fw-bold';
            showToast(`Connected to ${data.databaseName}`, 'success');
        } else {
            testResult.innerHTML = `<i class="fas fa-times-circle"></i> ${data.message}`;
            testResult.className = 'small text-danger';
            showToast(data.message, 'error');
        }
    } catch (error) {
        console.error('Test connection error:', error);
        testResult.innerHTML = `<i class="fas fa-times-circle"></i> Error: ${error.message}`;
        testResult.className = 'small text-danger';
        showToast(`Error: ${error.message}`, 'error');
    }
}

async function saveDbConfig() {
    const dbType = document.querySelector('input[name="modalDbType"]:checked').value;
    const connString = document.getElementById('modalConnString').value.trim();

    if (!connString) {
        showToast('กรุณากรอก Connection String', 'warning');
        return;
    }

    try {
        const response = await fetch('/Generator/SwitchDatabase', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                dbType: dbType,
                connectionString: connString
            })
        });

        const data = await response.json();

        if (data.success) {
            showToast(`Switched to ${dbType}: ${data.databaseName}`, 'success');
            
            // Clear table list and field list
            clearTableAndFieldLists();
            
            // Refresh status and reload table list
            checkDbStatus();

            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('dbConfigModal'));
            modal.hide();
        } else {
            showToast(`Failed: ${data.message}`, 'error');
        }
    } catch (error) {
        showToast(`Error: ${error.message}`, 'error');
    }
}

// ==========================================
// Database Configuration Management (LocalStorage)
// ==========================================
const STORAGE_KEY = 'angular_generator_db_configs';

// Initialize on load
function loadSavedConfigurations() {
    try {
        const configs = getLocalConfigs();
        displaySavedConfigurations(configs);
    } catch (error) {
        console.error('Failed to load saved configurations:', error);
    }
}

function getLocalConfigs() {
    const data = localStorage.getItem(STORAGE_KEY);
    return data ? JSON.parse(data) : [];
}

function saveLocalConfigs(configs) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(configs));
}

function displaySavedConfigurations(configs) {
    const container = document.getElementById('savedConfigsList');
    if (!container) return;

    if (!configs || configs.length === 0) {
        container.innerHTML = '<small class="text-muted fst-italic">No saved configurations in your browser. Create one below.</small>';
        return;
    }

    container.innerHTML = '';

    // Sort: Default first, then by LastUsed
    const sortedConfigs = [...configs].sort((a, b) => {
        if (a.isDefault) return -1;
        if (b.isDefault) return 1;
        return new Date(b.lastUsed) - new Date(a.lastUsed);
    });

    sortedConfigs.forEach(config => {
        const configItem = document.createElement('div');
        configItem.className = 'saved-config-item border rounded p-2 mb-2 bg-light';
        configItem.innerHTML = `
            <div class="d-flex justify-content-between align-items-start">
                <div class="flex-grow-1">
                    <div class="fw-bold text-primary">
                        ${config.name}
                        ${config.isDefault ? '<span class="badge bg-success ms-2">Default</span>' : ''}
                    </div>
                    <small class="text-muted">${config.dbType}</small>
                    ${config.description ? `<div><small class="text-muted fst-italic">${config.description}</small></div>` : ''}
                    <div><small class="text-muted" style="font-size: 0.7rem;">Connection: ${maskConnectionString(config.connectionString)}</small></div>
                </div>
                <div class="btn-group btn-group-sm ms-2">
                    <button class="btn btn-outline-primary" onclick="loadConfiguration('${config.name.replace(/'/g, "\\'")}')" title="Load to Form">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-danger" onclick="deleteConfiguration('${config.name.replace(/'/g, "\\'")}')" title="Delete">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
        container.appendChild(configItem);
    });
}

function saveCurrentConfiguration() {
    const configName = document.getElementById('saveConfigName')?.value.trim();
    const configDescription = document.getElementById('saveConfigDescription')?.value.trim() || '';
    const setAsDefault = document.getElementById('saveConfigDefault')?.checked || false;
    
    const dbType = document.querySelector('input[name="modalDbType"]:checked').value;
    const connString = document.getElementById('modalConnString').value.trim();

    if (!configName) {
        showToast('กรุณาใส่ชื่อ Configuration', 'warning');
        return;
    }

    if (!connString) {
        showToast('กรุณากรอก Connection String', 'warning');
        return;
    }

    try {
        let configs = getLocalConfigs();
        
        // Find existing
        const existingIndex = configs.findIndex(c => c.name === configName);
        
        const newConfig = {
            name: configName,
            dbType: dbType,
            connectionString: connString,
            isDefault: setAsDefault,
            description: configDescription,
            lastUsed: new Date().toISOString()
        };

        if (setAsDefault) {
            configs.forEach(c => c.isDefault = false);
        }

        if (existingIndex >= 0) {
            configs[existingIndex] = newConfig;
        } else {
            configs.push(newConfig);
        }

        saveLocalConfigs(configs);
        
        showToast(`บันทึกการตั้งค่า '${configName}' เรียบร้อยแล้ว`, 'success');
        
        // Clear save form
        document.getElementById('saveConfigName').value = '';
        document.getElementById('saveConfigDescription').value = '';
        document.getElementById('saveConfigDefault').checked = false;
        
        // Reload list
        loadSavedConfigurations();
    } catch (error) {
        showToast(`Error: ${error.message}`, 'error');
    }
}

function loadConfiguration(configName) {
    try {
        const configs = getLocalConfigs();
        const config = configs.find(c => c.name === configName);

        if (config) {
            // Set the values in modal
            const dbRadio = document.querySelector(`input[name="modalDbType"][value="${config.dbType}"]`);
            if (dbRadio) {
                dbRadio.checked = true;
                updateDbFields();
            }
            document.getElementById('modalConnString').value = config.connectionString;
            document.getElementById('saveConfigName').value = config.name;
            document.getElementById('saveConfigDescription').value = config.description || '';
            document.getElementById('saveConfigDefault').checked = config.isDefault || false;

            showToast(`โหลดค่าจาก '${configName}' เรียบร้อย`, 'success');
        } else {
            showToast('ไม่พบข้อมูลการตั้งค่า', 'error');
        }
    } catch (error) {
        showToast(`Error: ${error.message}`, 'error');
    }
}

function deleteConfiguration(configName) {
    if (!confirm(`คุณแน่ใจหรือไม่ว่าต้องการลบการตั้งค่า "${configName}" ออกจากเบราเซอร์?`)) {
        return;
    }

    try {
        let configs = getLocalConfigs();
        const newConfigs = configs.filter(c => c.name !== configName);
        saveLocalConfigs(newConfigs);
        
        showToast('ลบการตั้งค่าเรียบร้อยแล้ว', 'success');
        loadSavedConfigurations();
    } catch (error) {
        showToast(`Error: ${error.message}`, 'error');
    }
}

function exportConfigurations(includePasswords = false) {
    try {
        const configs = getLocalConfigs();
        if (configs.length === 0) {
            showToast('ไม่มีข้อมูลการตั้งค่าให้ Export', 'warning');
            return;
        }

        let exportData = configs.map(c => ({
            ...c,
            connectionString: includePasswords ? c.connectionString : maskConnectionString(c.connectionString)
        }));

        const json = JSON.stringify(exportData, null, 2);
        const blob = new Blob([json], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `db-configs-${new Date().toISOString().split('T')[0]}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);

        showToast('Export ข้อมูลเรียบร้อยแล้ว', 'success');
    } catch (error) {
        showToast(`Export failed: ${error.message}`, 'error');
    }
}

function maskConnectionString(conn) {
    if (!conn) return '';
    return conn.replace(/(Password|Pwd|pwd|password|User Id|Uid)=([^;]+)/gi, '$1=****');
}

// --- AS400 UI Helpers ---
function toggleAs400UI(isAs400) {
    const standardSection = document.getElementById('standardTableSection');
    const as400Section = document.getElementById('as400SpecialSection');

    if (isAs400) {
        if (standardSection) standardSection.style.display = 'none';
        if (as400Section) as400Section.style.display = 'block';
        // AS400 mode: use taller line-height for dropdown text
        document.querySelectorAll('.ui.dropdown .text').forEach(el => el.classList.add('as400-mode'));
    } else {
        if (standardSection) standardSection.style.display = 'block';
        if (as400Section) as400Section.style.display = 'none';
        // Normal mode: remove AS400 line-height
        document.querySelectorAll('.ui.dropdown .text').forEach(el => el.classList.remove('as400-mode'));
    }
}

function syncAs400TableName() {
    const lib = document.getElementById('libraryInput').value.trim();
    const table = document.getElementById('tableNameAs400Input').value.trim();
    const input = document.getElementById('tableNameInput');

    if (lib && table) {
        if (input) input.value = `${lib}.${table}`.toUpperCase();
    } else if (table) {
        if (input) input.value = table.toUpperCase();
    }
}

function syncAs400FromManual() {
    if (currentDbType !== 'AS400') return;

    const combined = getTableName();
    const libInput = document.getElementById('libraryInput');
    const tableInput = document.getElementById('tableNameAs400Input');

    if (combined && combined.includes('.')) {
        const parts = combined.split('.');
        if (libInput) libInput.value = parts[0].trim().toUpperCase();
        if (tableInput) tableInput.value = parts[1].trim().toUpperCase();
    } else if (tableInput) {
        tableInput.value = combined.toUpperCase();
    }
}
