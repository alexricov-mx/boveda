<script setup lang="ts">
import useSupplyOrdersQuery from '~/features/orden-surtimiento/actions/queries/use-supply-orders'
import OsStatusDot from '~/features/orden-surtimiento/components/OsStatusDot.vue'
import OsPagination from '~/features/orden-surtimiento/components/OsPagination.vue'
import OsSignatureDialog from '~/features/orden-surtimiento/components/OsSignatureDialog.vue'
import type { SupplyOrderDto } from '~/features/orden-surtimiento/model/supply-order'

const pageNum = ref(1)
const search = ref('')
const searchInput = ref('')
const selected = ref<SupplyOrderDto[]>([])
const signDialog = ref(false)

const { data, isFetching, isError } = useSupplyOrdersQuery({ pageNum, search })

const items = computed(() => data.value?.data ?? [])
const pager = computed(() => data.value?.pager)

const headers = [
  { title: '', key: 'select', width: 48, sortable: false },
  { title: 'OS / SAP', key: 'sapOrder', sortable: false },
  { title: 'Contrato', key: 'contract', sortable: false },
  { title: 'Organismo', key: 'organismClave', sortable: false },
  { title: 'Tipo', key: 'documentType', sortable: false },
  { title: 'Acreedor', key: 'creditor', sortable: false },
  { title: 'Total', key: 'total', align: 'end', sortable: false },
  { title: 'Moneda', key: 'currency', sortable: false },
  { title: 'Func.', key: 'functionarySignDate', width: 56, align: 'center', sortable: false },
  { title: 'Lib. VP', key: 'liberacionVPDate', width: 64, align: 'center', sortable: false },
] as const

function onSearch() {
  search.value = searchInput.value
  pageNum.value = 1
}

function onClearSearch() {
  searchInput.value = ''
  search.value = ''
  pageNum.value = 1
}

function toggleSelect(item: SupplyOrderDto) {
  const idx = selected.value.findIndex((s) => s.supplyOrderID === item.supplyOrderID)
  if (idx >= 0) selected.value.splice(idx, 1)
  else selected.value.push(item)
}

function isSelected(item: SupplyOrderDto) {
  return selected.value.some((s) => s.supplyOrderID === item.supplyOrderID)
}

function openSignDialog() {
  if (selected.value.length === 0) return
  signDialog.value = true
}

function onSigned() {
  signDialog.value = false
  selected.value = []
}
</script>

<template>
  <v-container fluid class="pa-4">
    <!-- Header -->
    <div class="d-flex align-center justify-space-between flex-wrap gap-2 mb-4">
      <div>
        <h2 class="text-h6 font-weight-bold text-primary">Órdenes de Surtimiento</h2>
        <p class="text-caption text-medium-emphasis">Firma electrónica de órdenes de surtimiento</p>
      </div>
      <v-btn
        color="primary"
        prepend-icon="mdi-draw-pen"
        :disabled="selected.length === 0"
        @click="openSignDialog"
      >
        Firmar ({{ selected.length }})
      </v-btn>
    </div>

    <!-- Search -->
    <v-card variant="outlined" class="mb-4 pa-3">
      <div class="d-flex gap-2">
        <v-text-field
          v-model="searchInput"
          placeholder="Buscar por OS, contrato, acreedor..."
          prepend-inner-icon="mdi-magnify"
          hide-details
          clearable
          class="flex-grow-1"
          @keyup.enter="onSearch"
          @click:clear="onClearSearch"
        />
        <v-btn color="primary" variant="flat" @click="onSearch">Buscar</v-btn>
      </div>
    </v-card>

    <!-- Table -->
    <v-card variant="outlined">
      <v-progress-linear v-if="isFetching" indeterminate color="primary" height="2" />

      <v-alert v-if="isError" type="error" variant="tonal" class="ma-4" density="compact">
        Error al cargar las órdenes de surtimiento. Intente de nuevo.
      </v-alert>

      <v-table v-else density="compact" hover>
        <thead>
          <tr>
            <th
              v-for="h in headers"
              :key="h.key"
              :style="h.width ? `width:${h.width}px` : ''"
              :class="(h as any).align === 'end' ? 'text-right' : (h as any).align === 'center' ? 'text-center' : ''"
              class="text-caption font-weight-bold text-medium-emphasis"
            >
              {{ h.title }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!isFetching && items.length === 0">
            <td :colspan="headers.length" class="text-center text-medium-emphasis py-8">
              <v-icon size="40" class="mb-2 d-block">mdi-inbox-outline</v-icon>
              No hay órdenes de surtimiento pendientes
            </td>
          </tr>
          <tr
            v-for="item in items"
            :key="item.supplyOrderID"
            :class="{ 'bg-blue-lighten-5': isSelected(item) }"
            style="cursor: pointer"
            @click="toggleSelect(item)"
          >
            <td>
              <v-checkbox-btn
                :model-value="isSelected(item)"
                color="primary"
                density="compact"
                @click.stop="toggleSelect(item)"
              />
            </td>
            <td class="text-body-2 font-weight-medium">{{ item.sapOrder }}</td>
            <td class="text-body-2">{{ item.contract }}</td>
            <td class="text-body-2">{{ item.organismClave }}</td>
            <td>
              <v-chip size="x-small" variant="tonal" color="info" label>
                {{ item.documentType }}
              </v-chip>
            </td>
            <td class="text-body-2">{{ item.creditor }}</td>
            <td class="text-right text-body-2 font-weight-medium">{{ item.total }}</td>
            <td>
              <v-chip size="x-small" variant="outlined" label>{{ item.currency }}</v-chip>
            </td>
            <td class="text-center">
              <OsStatusDot :sign-date="item.functionarySignDate" tooltip="Firma funcionario" />
            </td>
            <td class="text-center">
              <OsStatusDot :sign-date="item.liberacionVPDate" tooltip="Liberación VP" />
            </td>
          </tr>
        </tbody>
      </v-table>

      <v-divider v-if="pager" />
      <div v-if="pager" class="pa-3">
        <OsPagination :pager="pager" @change="(p) => (pageNum = p)" />
      </div>
    </v-card>

    <OsSignatureDialog
      v-model="signDialog"
      :documentos="selected"
      @signed="onSigned"
    />
  </v-container>
</template>
