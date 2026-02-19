import { useState, useEffect } from 'react'
import './App.css'

// Helper para fazer fetch com tratamento de JSON seguro
async function safeFetch<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, options)

  if (!response.ok) {
    const contentType = response.headers.get('content-type')
    if (contentType && contentType.includes('application/json')) {
      const errorData = await response.json()
      throw new Error(errorData.error || errorData.message || `HTTP Error ${response.status}`)
    } else {
      throw new Error(`HTTP Error ${response.status}`)
    }
  }

  const contentType = response.headers.get('content-type')
  if (contentType && contentType.includes('application/json')) {
    return await response.json()
  }

  return null as T
}

interface Customer {
  id: string
  personType: number
  name: string
  cpf: string | null
  cnpj: string | null
  birthDate: string | null
  foundationDate: string | null
  phone: string
  email: string
  address: {
    zipCode: string
    street: string
    number: string
    complement: string | null
    neighborhood: string
    city: string
    state: string
  }
  stateRegistration: string | null
  isStateRegistrationExempt: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

interface ZipCodeInfo {
  cep: string
  logradouro: string
  complemento: string
  bairro: string
  localidade: string
  uf: string
  erro: boolean
}

function App() {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [filteredCustomers, setFilteredCustomers] = useState<Customer[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [zipCodeLoading, setZipCodeLoading] = useState(false)
  const [personType, setPersonType] = useState<'natural' | 'legal'>('natural')
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all')
  const [sortField, setSortField] = useState<'name' | 'createdAt'>('name')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage, setItemsPerPage] = useState(10)

  const [formData, setFormData] = useState({
    name: '',
    cpf: '',
    cnpj: '',
    birthDate: '',
    foundationDate: '',
    phone: '',
    email: '',
    stateRegistration: '',
    isStateRegistrationExempt: false,
    address: {
      zipCode: '',
      street: '',
      number: '',
      complement: '',
      neighborhood: '',
      city: '',
      state: ''
    }
  })

  const fetchCustomers = async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await safeFetch<Customer[]>('/api/customers')
      setCustomers(data || [])
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao carregar clientes')
      console.error('Erro ao buscar clientes:', err)
    } finally {
      setLoading(false)
    }
  }

  const fetchZipCode = async (zipCode: string) => {
    if (zipCode.length !== 8) return

    setZipCodeLoading(true)

    try {
      const response = await fetch(`/api/zipcode/${zipCode}`)

      if (!response.ok) {
        throw new Error('CEP não encontrado')
      }

      const contentType = response.headers.get('content-type')
      if (!contentType || !contentType.includes('application/json')) {
        throw new Error('Resposta inválida do servidor')
      }

      const data: ZipCodeInfo = await response.json()

      if (!data.erro) {
        setFormData(prev => ({
          ...prev,
          address: {
            ...prev.address,
            street: data.logradouro,
            neighborhood: data.bairro,
            city: data.localidade,
            state: data.uf
          }
        }))
      }
    } catch (err) {
      console.error('Erro ao buscar CEP:', err)
    } finally {
      setZipCodeLoading(false)
    }
  }

  const handleZipCodeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const zipCode = e.target.value.replace(/\D/g, '')
    setFormData(prev => ({
      ...prev,
      address: { ...prev.address, zipCode }
    }))
    
    if (zipCode.length === 8) {
      fetchZipCode(zipCode)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      if (editingCustomer) {
        // Update existing customer
        const response = await fetch(`/api/customers/${editingCustomer.id}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            name: formData.name,
            phone: formData.phone,
            email: formData.email,
            address: formData.address,
            stateRegistration: formData.stateRegistration || null,
            isStateRegistrationExempt: formData.isStateRegistrationExempt
          })
        })

        if (!response.ok) {
          const contentType = response.headers.get('content-type')
          if (contentType && contentType.includes('application/json')) {
            const errorData = await response.json()
            throw new Error(errorData.error || 'Erro ao atualizar cliente')
          } else {
            throw new Error(`Erro HTTP: ${response.status}`)
          }
        }
      } else {
        // Create new customer
        const endpoint = personType === 'natural' 
          ? '/api/customers/natural-person' 
          : '/api/customers/legal-entity'

        const payload = personType === 'natural' 
          ? {
              name: formData.name,
              cpf: formData.cpf,
              birthDate: formData.birthDate,
              phone: formData.phone,
              email: formData.email,
              address: formData.address
            }
          : {
              name: formData.name,
              cnpj: formData.cnpj,
              foundationDate: formData.foundationDate,
              phone: formData.phone,
              email: formData.email,
              address: formData.address,
              stateRegistration: formData.stateRegistration || null,
              isStateRegistrationExempt: formData.isStateRegistrationExempt
            }

        const response = await fetch(endpoint, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload)
        })

        if (!response.ok) {
          const errorData = await response.json()
          throw new Error(errorData.error || errorData.errors?.join(', ') || 'Erro ao criar cliente')
        }
      }

      await fetchCustomers()
      setShowForm(false)
      setEditingCustomer(null)
      setFormData({
        name: '',
        cpf: '',
        cnpj: '',
        birthDate: '',
        foundationDate: '',
        phone: '',
        email: '',
        stateRegistration: '',
        isStateRegistrationExempt: false,
        address: {
          zipCode: '',
          street: '',
          number: '',
          complement: '',
          neighborhood: '',
          city: '',
          state: ''
        }
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao salvar cliente')
      console.error('Erro ao salvar cliente:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = (customer: Customer) => {
    setEditingCustomer(customer)
    setPersonType(customer.personType === 1 ? 'natural' : 'legal')
    setFormData({
      name: customer.name,
      cpf: customer.cpf || '',
      cnpj: customer.cnpj || '',
      birthDate: customer.birthDate || '',
      foundationDate: customer.foundationDate || '',
      phone: customer.phone,
      email: customer.email,
      stateRegistration: customer.stateRegistration || '',
      isStateRegistrationExempt: customer.isStateRegistrationExempt,
      address: {
        zipCode: customer.address.zipCode,
        street: customer.address.street,
        number: customer.address.number,
        complement: customer.address.complement || '',
        neighborhood: customer.address.neighborhood,
        city: customer.address.city,
        state: customer.address.state
      }
    })
    setShowForm(true)
  }

  const handleToggleActive = async (customerId: string, isActive: boolean) => {
    if (!confirm(`Deseja realmente ${isActive ? 'desativar' : 'ativar'} este cliente?`)) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      const endpoint = isActive 
        ? `/api/customers/${customerId}/deactivate`
        : `/api/customers/${customerId}/activate`

      const response = await fetch(endpoint, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        }
      })

      if (!response.ok) {
        const contentType = response.headers.get('content-type')
        if (contentType && contentType.includes('application/json')) {
          const errorData = await response.json()
          throw new Error(errorData.error || `Erro ao ${isActive ? 'desativar' : 'ativar'} cliente`)
        } else {
          throw new Error(`Erro HTTP: ${response.status}`)
        }
      }

      // Sucesso - recarregar lista
      await fetchCustomers()
    } catch (err) {
      setError(err instanceof Error ? err.message : `Falha ao ${isActive ? 'desativar' : 'ativar'} cliente`)
      console.error('Erro:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleCancelEdit = () => {
    setShowForm(false)
    setEditingCustomer(null)
    setFormData({
      name: '',
      cpf: '',
      cnpj: '',
      birthDate: '',
      foundationDate: '',
      phone: '',
      email: '',
      stateRegistration: '',
      isStateRegistrationExempt: false,
      address: {
        zipCode: '',
        street: '',
        number: '',
        complement: '',
        neighborhood: '',
        city: '',
        state: ''
      }
    })
  }

  useEffect(() => {
    fetchCustomers()
  }, [])

  useEffect(() => {
    let filtered = [...customers]

    // Filtro de busca
    if (searchTerm) {
      filtered = filtered.filter(c => 
        c.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        c.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (c.cpf && c.cpf.includes(searchTerm)) ||
        (c.cnpj && c.cnpj.includes(searchTerm))
      )
    }

    // Filtro de status
    if (filterStatus === 'active') {
      filtered = filtered.filter(c => c.isActive)
    } else if (filterStatus === 'inactive') {
      filtered = filtered.filter(c => !c.isActive)
    }

    // Ordenação
    filtered.sort((a, b) => {
      let comparison = 0
      if (sortField === 'name') {
        comparison = a.name.localeCompare(b.name)
      } else if (sortField === 'createdAt') {
        comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      }
      return sortDirection === 'asc' ? comparison : -comparison
    })

    setFilteredCustomers(filtered)
    setCurrentPage(1) // Reset para primeira página quando filtros mudam
  }, [customers, searchTerm, filterStatus, sortField, sortDirection])

  // Paginação
  const totalPages = Math.ceil(filteredCustomers.length / itemsPerPage)
  const startIndex = (currentPage - 1) * itemsPerPage
  const endIndex = startIndex + itemsPerPage
  const paginatedCustomers = filteredCustomers.slice(startIndex, endIndex)

  const handlePageChange = (page: number) => {
    setCurrentPage(page)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const handleItemsPerPageChange = (value: number) => {
    setItemsPerPage(value)
    setCurrentPage(1)
  }

  const handleSort = (field: 'name' | 'createdAt') => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDirection('asc')
    }
  }

  const formatCnpj = (cnpj: string) => {
    return cnpj.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5')
  }

  const formatCpf = (cpf: string) => {
    return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
  }

  const formatPhone = (phone: string) => {
    if (phone.length === 11) {
      return phone.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3')
    }
    return phone.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3')
  }

  const formatZipCode = (zipCode: string) => {
    return zipCode.replace(/(\d{5})(\d{3})/, '$1-$2')
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>🏢 CRM - Gerenciamento de Clientes</h1>
      </header>

      {error && (
        <div className="alert alert-error">
          {error}
        </div>
      )}

      {showForm && (
        <div className="card form-card">
          <div className="form-type-tabs">
            <button
              type="button"
              className={`tab ${personType === 'natural' ? 'active' : ''}`}
              onClick={() => setPersonType('natural')}
            >
              👤 Pessoa Física
            </button>
            <button
              type="button"
              className={`tab ${personType === 'legal' ? 'active' : ''}`}
              onClick={() => setPersonType('legal')}
            >
              🏢 Pessoa Jurídica
            </button>
          </div>

          <h2>
            {editingCustomer 
              ? 'Editar Cliente' 
              : (personType === 'natural' ? 'Novo Cliente - Pessoa Física' : 'Novo Cliente - Pessoa Jurídica')
            }
          </h2>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="name">{personType === 'natural' ? 'Nome Completo' : 'Razão Social'}</label>
              <input
                type="text"
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
              />
            </div>

            {!editingCustomer && (
              <div className="form-row">
                {personType === 'natural' ? (
                  <>
                    <div className="form-group">
                      <label htmlFor="cpf">CPF</label>
                      <input
                        type="text"
                        id="cpf"
                        value={formData.cpf}
                        onChange={(e) => setFormData({ ...formData, cpf: e.target.value.replace(/\D/g, '') })}
                        placeholder="12345678909"
                        maxLength={11}
                        required
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="birthDate">Data de Nascimento</label>
                      <input
                        type="date"
                        id="birthDate"
                        value={formData.birthDate}
                        onChange={(e) => setFormData({ ...formData, birthDate: e.target.value })}
                        required
                      />
                    </div>
                  </>
                ) : (
                  <>
                    <div className="form-group">
                      <label htmlFor="cnpj">CNPJ</label>
                      <input
                        type="text"
                        id="cnpj"
                        value={formData.cnpj}
                        onChange={(e) => setFormData({ ...formData, cnpj: e.target.value.replace(/\D/g, '') })}
                        placeholder="12345678000195"
                        maxLength={14}
                        required
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="foundationDate">Data de Fundação</label>
                      <input
                        type="date"
                        id="foundationDate"
                        value={formData.foundationDate}
                        onChange={(e) => setFormData({ ...formData, foundationDate: e.target.value })}
                        required
                      />
                    </div>
                  </>
                )}
              </div>
            )}

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="phone">Telefone</label>
                <input
                  type="text"
                  id="phone"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value.replace(/\D/g, '') })}
                  placeholder="11987654321"
                  maxLength={11}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="email">Email</label>
                <input
                  type="email"
                  id="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  required
                />
              </div>
            </div>

            {personType === 'legal' && (
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="stateRegistration">Inscrição Estadual</label>
                  <input
                    type="text"
                    id="stateRegistration"
                    value={formData.stateRegistration}
                    onChange={(e) => setFormData({ ...formData, stateRegistration: e.target.value })}
                    disabled={formData.isStateRegistrationExempt}
                  />
                </div>
                <div className="form-group checkbox-group">
                  <label>
                    <input
                      type="checkbox"
                      checked={formData.isStateRegistrationExempt}
                      onChange={(e) => setFormData({ 
                        ...formData, 
                        isStateRegistrationExempt: e.target.checked,
                        stateRegistration: e.target.checked ? '' : formData.stateRegistration
                      })}
                    />
                    <span>Isento de Inscrição Estadual</span>
                  </label>
                </div>
              </div>
            )}

            <div className="form-group">
              <label htmlFor="zipCode">
                CEP {zipCodeLoading && <span className="loading-text">(buscando...)</span>}
              </label>
              <input
                type="text"
                id="zipCode"
                value={formData.address.zipCode}
                onChange={handleZipCodeChange}
                placeholder="01310100"
                maxLength={8}
                required
              />
            </div>

            <div className="form-row">
              <div className="form-group" style={{ flex: 3 }}>
                <label htmlFor="street">Logradouro</label>
                <input
                  type="text"
                  id="street"
                  value={formData.address.street}
                  onChange={(e) => setFormData({ 
                    ...formData, 
                    address: { ...formData.address, street: e.target.value }
                  })}
                  required
                />
              </div>

              <div className="form-group" style={{ flex: 1 }}>
                <label htmlFor="number">Número</label>
                <input
                  type="text"
                  id="number"
                  value={formData.address.number}
                  onChange={(e) => setFormData({ 
                    ...formData, 
                    address: { ...formData.address, number: e.target.value }
                  })}
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="complement">Complemento</label>
              <input
                type="text"
                id="complement"
                value={formData.address.complement}
                onChange={(e) => setFormData({ 
                  ...formData, 
                  address: { ...formData.address, complement: e.target.value }
                })}
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="neighborhood">Bairro</label>
                <input
                  type="text"
                  id="neighborhood"
                  value={formData.address.neighborhood}
                  onChange={(e) => setFormData({ 
                    ...formData, 
                    address: { ...formData.address, neighborhood: e.target.value }
                  })}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="city">Cidade</label>
                <input
                  type="text"
                  id="city"
                  value={formData.address.city}
                  onChange={(e) => setFormData({ 
                    ...formData, 
                    address: { ...formData.address, city: e.target.value }
                  })}
                  required
                />
              </div>

              <div className="form-group" style={{ flex: 0.5 }}>
                <label htmlFor="state">UF</label>
                <input
                  type="text"
                  id="state"
                  value={formData.address.state}
                  onChange={(e) => setFormData({ 
                    ...formData, 
                    address: { ...formData.address, state: e.target.value.toUpperCase() }
                  })}
                  maxLength={2}
                  required
                />
              </div>
            </div>

            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={handleCancelEdit}>
                Cancelar
              </button>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? 'Salvando...' : (editingCustomer ? 'Atualizar Cliente' : 'Salvar Cliente')}
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="customers-container">
        <div className="customers-header">
          <h2>Clientes Cadastrados</h2>
          <button 
            className="btn btn-primary"
            onClick={() => setShowForm(!showForm)}
          >
            {showForm ? 'Cancelar' : '+ Novo Cliente'}
          </button>
        </div>

        <div className="filters">
            <div className="search-box">
              <input
                type="text"
                placeholder="🔍 Buscar por nome, email, CPF ou CNPJ..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="search-input"
              />
            </div>

            <div className="filter-buttons">
              <button
                className={`filter-btn ${filterStatus === 'all' ? 'active' : ''}`}
                onClick={() => setFilterStatus('all')}
              >
                Todos ({customers.length})
              </button>
              <button
                className={`filter-btn ${filterStatus === 'active' ? 'active' : ''}`}
                onClick={() => setFilterStatus('active')}
              >
                Ativos ({customers.filter(c => c.isActive).length})
              </button>
              <button
                className={`filter-btn ${filterStatus === 'inactive' ? 'active' : ''}`}
                onClick={() => setFilterStatus('inactive')}
              >
                Inativos ({customers.filter(c => !c.isActive).length})
              </button>
            </div>
          </div>
        </div>

        {loading && !showForm && <div className="loading">Carregando...</div>}

        {!loading && filteredCustomers.length === 0 && customers.length === 0 && (
          <div className="empty-state">
            <p>Nenhum cliente cadastrado ainda.</p>
            <p>Clique em "Novo Cliente" para começar.</p>
          </div>
        )}

        {!loading && filteredCustomers.length === 0 && customers.length > 0 && (
          <div className="empty-state">
            <p>Nenhum cliente encontrado com os filtros aplicados.</p>
          </div>
        )}

        {filteredCustomers.length > 0 && (
          <>
            <div className="table-container">
              <table className="customers-table">
                <thead>
                  <tr>
                    <th>Tipo</th>
                    <th onClick={() => handleSort('name')} className="sortable">
                      Nome {sortField === 'name' && (sortDirection === 'asc' ? '↑' : '↓')}
                    </th>
                    <th>Documento</th>
                    <th>Contato</th>
                    <th>Endereço</th>
                    <th onClick={() => handleSort('createdAt')} className="sortable">
                      Cadastrado em {sortField === 'createdAt' && (sortDirection === 'asc' ? '↑' : '↓')}
                    </th>
                    <th>Status</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedCustomers.map(customer => (
                  <tr key={customer.id} className={!customer.isActive ? 'inactive-row' : ''}>
                    <td>
                      <span className={`badge ${customer.personType === 1 ? 'badge-person' : 'badge-company'}`}>
                        {customer.personType === 1 ? '👤 PF' : '🏢 PJ'}
                      </span>
                    </td>
                    <td className="name-cell">
                      <strong>{customer.name}</strong>
                    </td>
                    <td>
                      {customer.cpf && <div className="doc-info">CPF: {formatCpf(customer.cpf)}</div>}
                      {customer.cnpj && <div className="doc-info">CNPJ: {formatCnpj(customer.cnpj)}</div>}
                      {customer.stateRegistration && <div className="doc-info small">IE: {customer.stateRegistration}</div>}
                      {customer.isStateRegistrationExempt && <div className="doc-info small">IE: Isento</div>}
                    </td>
                    <td>
                      <div className="contact-info">
                        <div>📧 {customer.email}</div>
                        <div>📱 {formatPhone(customer.phone)}</div>
                      </div>
                    </td>
                    <td>
                      <div className="address-info">
                        {customer.address.street}, {customer.address.number}
                        <br />
                        {customer.address.city}/{customer.address.state}
                        <br />
                        <small>CEP: {formatZipCode(customer.address.zipCode)}</small>
                      </div>
                    </td>
                    <td>
                      <div className="date-info">
                        {new Date(customer.createdAt).toLocaleDateString('pt-BR')}
                        {customer.updatedAt && (
                          <small>Atualizado: {new Date(customer.updatedAt).toLocaleDateString('pt-BR')}</small>
                        )}
                      </div>
                    </td>
                    <td>
                      <span className={`badge ${customer.isActive ? 'badge-active' : 'badge-inactive'}`}>
                        {customer.isActive ? 'Ativo' : 'Inativo'}
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button 
                          className="btn-icon btn-edit"
                          onClick={() => handleEdit(customer)}
                          title="Editar cliente"
                        >
                          ✏️
                        </button>
                        <button 
                          className={`btn-icon ${customer.isActive ? 'btn-deactivate' : 'btn-activate'}`}
                          onClick={() => handleToggleActive(customer.id, customer.isActive)}
                          title={customer.isActive ? 'Desativar cliente' : 'Ativar cliente'}
                        >
                          {customer.isActive ? '🔒' : '✅'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="table-footer">
              <div className="footer-left">
                <div className="items-per-page">
                  <label htmlFor="itemsPerPage">Itens por página:</label>
                  <select
                    id="itemsPerPage"
                    value={itemsPerPage}
                    onChange={(e) => handleItemsPerPageChange(Number(e.target.value))}
                    className="items-select"
                  >
                    <option value={1}>1</option>
                    <option value={5}>5</option>
                    <option value={10}>10</option>
                    <option value={25}>25</option>
                    <option value={50}>50</option>
                    <option value={100}>100</option>
                  </select>
                </div>
                <div className="footer-info">
                  Mostrando {startIndex + 1} a {Math.min(endIndex, filteredCustomers.length)} de {filteredCustomers.length} clientes
                </div>
              </div>

              {totalPages > 1 && (
                <div className="pagination">
              <button
                className="pagination-btn"
                onClick={() => handlePageChange(1)}
                disabled={currentPage === 1}
                title="Primeira página"
              >
                &laquo;&laquo;
              </button>
              <button
                className="pagination-btn"
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                title="Página anterior"
              >
                &laquo;
              </button>

              {Array.from({ length: totalPages }, (_, i) => i + 1)
                .filter(page => {
                  // Mostra: primeira, última, atual e 2 ao redor da atual
                  return page === 1 || 
                         page === totalPages || 
                         Math.abs(page - currentPage) <= 2
                })
                .map((page, index, array) => {
                  // Adiciona "..." entre números não consecutivos
                  const showEllipsis = index > 0 && page - array[index - 1] > 1

                  return (
                    <div key={page} style={{ display: 'flex', gap: '0.25rem' }}>
                      {showEllipsis && <span className="pagination-ellipsis">...</span>}
                      <button
                        className={`pagination-btn ${currentPage === page ? 'active' : ''}`}
                        onClick={() => handlePageChange(page)}
                      >
                        {page}
                      </button>
                    </div>
                  )
                })}

              <button
                className="pagination-btn"
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                title="Próxima página"
              >
                &raquo;
              </button>
              <button
                className="pagination-btn"
                onClick={() => handlePageChange(totalPages)}
                disabled={currentPage === totalPages}
                title="Última página"
              >
                &raquo;&raquo;
              </button>
            </div>
            )}
          </div>
          </div>
          </>
        )}
      </div>   
  )
}

export default App
