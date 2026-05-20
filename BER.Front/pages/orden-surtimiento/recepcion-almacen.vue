<script setup lang="ts">
import useReceptionAlmacenQuery from '~/features/orden-surtimiento/actions/queries/use-reception-almacen'
import OsStatusDot from '~/features/orden-surtimiento/components/OsStatusDot.vue'
import OsPagination from '~/features/orden-surtimiento/components/OsPagination.vue'
import OsSignatureDialog from '~/features/orden-surtimiento/components/OsSignatureDialog.vue'
import type { ReceptionDto } from '~/features/orden-surtimiento/model/reception-almacen'

const pageNum = ref(1)
const search = ref('')
const searchInput = ref('')
const selected = ref<ReceptionDto[]>([])
const signDialog = ref(false)

const { data, isFetching, isError } = useReceptionAlmacenQuery({ pageNum, search })

const items = computed(() => data.value?.data ?? [])
const pager = computed(() => data.value?.pager)

const headers = [
  { title: '', key: 'select', width: 48 },
  { title: 'Recepción', key: 'reception' },
  { title: 'SIAF', key: 'siafOrder' },
  { title: 'OS / SAP', key: 'sapOrder' },
  { title: 'Contrato', key: 'contract' },
  { title: 'Organismo', key: 'clave' },
  { title: 'Total', key: 'total', align: 'end' },
  { title: 'Moneda', key: 'currency' },
  { title: 'Func.', key: 'functionarySignDate', width: 56, align: 'center' },
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

function toggleSelect(item: ReceptionDto) {
  const idx = selected.value.findIndex((s) => s.receptionID === item.receptionID)
  if (idx >= 0) selected.value.splice(idx, 1)
  else selected.value.push(item)
}

function isSelected(item: ReceptionDto) {
  return selected.value.some((s) => s.receptionID === item.receptionID)
}
</script>

<template>
  <v-container fluid class="pa-4">
    <div class="d-flex align-center justify-space-between flex-wrap gap-2 mb-4">
      <div>
        <h2 class="text-h6 font-weight-bold text-primary">Recepción Entrada de Mercancías</h2>
        <p class="text-caption text-medium-emphasis">Firma electrónica de recepciones de almacén</p>
      </div>
      <v-btn
        color="primary"
        prepend-icon="mdi-draw-pen"
        :disabled="selected.length === 0"
        @click="signDialog = true"
      >
        Firmar ({{ selected.length }})
      </v-btn>
    </div>

    <v-card variant="outlined" class="mb-4 pa-3">
      <div class="d-flex gap-2">
        <v-text-field
          v-model="searchInput"
          placeholder="Buscar por recepción, contrato, organismo..."
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

    <v-card variant="outlined">
      <v-progress-linear v-if="isFetching" indeterminate color="primary" height="2" />

      <v-alert v-if="isError" type="error" variant="tonal" class="ma-4" density="compact">
        Error al cargar las recepciones. Intente de nuevo.
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
              No hay recepciones pendientes
            </td>
          </tr>
          <tr
            v-for="item in items"
            :key="item.receptionID"
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
            <td class="text-body-2 font-weight-medium">{{ item.reception }}</td>
            <td class="text-body-2">{{ item.siafOrder }}</td>
            <td class="text-body-2">{{ item.sapOrder }}</td>
            <td class="text-body-2">{{ item.contract }}</td>
            <td class="text-body-2">{{ item.clave }}</td>
            <td class="text-right text-body-2 font-weight-medium">{{ item.total }}</td>
            <td>
              <v-chip size="x-small" variant="outlined" label>{{ item.currency }}</v-chip>
            </td>
            <td class="text-center">
              <OsStatusDot
                :sign-date="item.functionary ? item.functionarySignDate : null"
                tooltip="Firma funcionario"
              />
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
      @signed="signDialog = false; selected = []"
    />
  </v-container>
</template>
