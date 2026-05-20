<script setup lang="ts">
const seguridadStore = useSeguridadStore()
const brandingStore = useBrandingStore()
const config = useRuntimeConfig()

const drawer = ref(true)

const legacyUrl = config.public.legacyAppUrl as string

function legacyLink(path: string) {
  return legacyUrl ? `${legacyUrl}${path}` : '#'
}

const menuItems = [
  {
    group: 'Órdenes de Surtimiento',
    icon: 'mdi-clipboard-list',
    items: [
      {
        title: 'Órdenes de Surtimiento',
        icon: 'mdi-star-four-points-circle',
        to: '/orden-surtimiento',
        badge: 'Nuevo',
      },
      {
        title: 'Recepción Almacén',
        icon: 'mdi-star-four-points-circle',
        to: '/orden-surtimiento/recepcion-almacen',
        badge: 'Nuevo',
      },
      {
        title: 'Estimación de Obra',
        icon: 'mdi-star-four-points-circle',
        to: '/orden-surtimiento/estimacion-obra',
        badge: 'Nuevo',
      },
      { divider: true },
      {
        title: 'Órdenes de Surtimiento',
        icon: 'mdi-open-in-new',
        href: legacyLink('/OrdenSurtimiento'),
        badge: 'Legado',
      },
      {
        title: 'Recepción Almacén',
        icon: 'mdi-open-in-new',
        href: legacyLink('/ReceptionAlmacen'),
        badge: 'Legado',
      },
      {
        title: 'Estimación de Obra',
        icon: 'mdi-open-in-new',
        href: legacyLink('/EstimacionObra'),
        badge: 'Legado',
      },
    ],
  },
]
</script>

<template>
  <v-app>
    <v-app-bar color="primary" elevation="2" density="compact">
      <v-app-bar-nav-icon @click="drawer = !drawer" />
      <v-app-bar-title>
        <span class="font-weight-bold text-body-1">{{ brandingStore.applicationShortName }}</span>
        <span class="text-caption ml-2 opacity-70 d-none d-sm-inline">
          {{ brandingStore.applicationLongName }}
        </span>
      </v-app-bar-title>
      <v-spacer />
      <v-menu>
        <template #activator="{ props }">
          <v-btn icon v-bind="props" variant="text">
            <v-icon>mdi-account-circle</v-icon>
          </v-btn>
        </template>
        <v-list density="compact" min-width="220">
          <v-list-item>
            <v-list-item-title class="text-body-2 font-weight-medium">
              {{ seguridadStore.usuarioAutenticado?.nombre }}
              {{ seguridadStore.usuarioAutenticado?.apellidos }}
            </v-list-item-title>
            <v-list-item-subtitle class="text-caption">
              {{ seguridadStore.usuarioAutenticado?.email }}
            </v-list-item-subtitle>
          </v-list-item>
          <v-divider />
          <v-list-item
            prepend-icon="mdi-logout"
            title="Cerrar sesión"
            variant="text"
            @click="seguridadStore.logout()"
          />
        </v-list>
      </v-menu>
    </v-app-bar>

    <v-navigation-drawer v-model="drawer" color="grey-lighten-5" width="280">
      <div class="pa-4 pb-2">
        <v-img :src="brandingStore.corporationLogoUrl" height="40" contain class="mb-2" />
        <p class="text-caption text-medium-emphasis text-center">
          {{ brandingStore.corporationLongName }}
        </p>
      </div>
      <v-divider />

      <v-list density="compact" nav class="pt-2">
        <v-list-item
          prepend-icon="mdi-home-outline"
          title="Inicio"
          to="/"
          color="primary"
          rounded="lg"
        />

        <template v-for="group in menuItems" :key="group.group">
          <v-list-subheader class="text-caption font-weight-bold mt-2">
            {{ group.group }}
          </v-list-subheader>

          <template v-for="item in group.items" :key="item.title">
            <v-divider v-if="(item as any).divider" class="my-1" />
            <v-list-item
              v-else
              :prepend-icon="item.icon"
              :title="item.title"
              :to="(item as any).to"
              :href="(item as any).href"
              :target="(item as any).href ? '_blank' : undefined"
              color="primary"
              rounded="lg"
            >
              <template #append>
                <v-chip
                  :color="item.badge === 'Nuevo' ? 'success' : 'grey'"
                  size="x-small"
                  variant="flat"
                  label
                >
                  {{ item.badge }}
                </v-chip>
              </template>
            </v-list-item>
          </template>
        </template>
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <slot />
    </v-main>
  </v-app>
</template>
