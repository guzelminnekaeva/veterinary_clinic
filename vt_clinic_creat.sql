
-- вид животного(собака, кошка)
create table types_animal (
  id   int IDENTITY(1,1) not null primary key,
    --наименование вида животного
  name nchar(70) not null
)

-- порода
create table breeds (
  id   int IDENTITY(1,1) not null primary key,
    --наименование породы
  name nchar(70) not null
)

-- животные
create table animal (
  id int IDENTITY(1,1) not null primary key,
    --кличка
  name    nchar(70) not null,
    --вид животного(собака, кошка)
  type_animal_id int   not null foreign key references types_animal(id),
    --порода
  breed_id int   not null foreign key references breeds(id),
    --вес
  weight        int,
    --день рождения
  birthdate     date,
    --пол
  male          nchar(1) not null
)

-- хозяева
create table person (
  id int IDENTITY(1,1) not null primary key,
    --имя
  first_name    nchar(70) not null,
    --фамилия
  last_name     nchar(70) not null,
    --отчесвто
  patronymic     nchar(70),
    --день рождения
  birthdate     date     not null,
    --пол
  male          nchar(1) not null,
    --номер телефона
  phone    nchar(12),
    --почта
  email    nchar(100),
    --адрес
  address    nchar(70),
    --дата первого обращения
  start_date    date     not null,
    --логин
  login    nchar(40)  not null,
    --пароль
  password    nchar(20)  not null
)

-- специализации докторов
create table specialities (
  id   int IDENTITY(1,1) not null primary key,
    --наименование специализации
  name nchar(70) not null
)

-- доктора
create table doctor (
  id            int  IDENTITY(1,1) not null primary key,
    --имя
  first_name    nchar(70) not null,
    --фамилия
  last_name     nchar(70) not null,
    --отчесвто
  patronymic     nchar(70),
    --день рождения
  birthdate     date     not null,
    --пол
  male          nchar(1) not null,
  --check (male in ('M', 'F')),
    --занятость, ч (время работы в неделю)
  time_work      int not null,
    --дата, когда был нанят на работу
  start_date    date     not null,
    --дата, когда был уволен с работы
  end_date      date,
    --специализация
  speciality_id int   not null foreign key references specialities(id),
    --номер кабинета
  room      int not null,
    --номер телефона
  phone    nchar(12),
    --почта
  email    nchar(100),
    --адрес
  address    nchar(70)
)

-- расписание 
create table timetable (
  id           int IDENTITY(1,1) not null primary key,
  doctor_id   int not null foreign key references doctor(id),
    --дата приема доктора
  dt    date     not null,
    --время начала работы
  start_time       time   not null,
    --время конца работы
  end_time         time   not null
)

-- услуги
create table services_doctor (
  id   int IDENTITY(1,1) not null primary key,
    --наименование услуги
  name nchar(150) not null,
    --стоимость
  summ           int not null,
    -- минимальный вес
  weight_min        int,
    --максимальный вес
  weight_max        int
)

-- связка специалиста и услуг
create table specialities_services_map (
  specialities_id   int not null foreign key references specialities(id),
  services_doctor_id int not null foreign key references services_doctor(id)
)

-- прививки
create table vaccinations (
  id   int IDENTITY(1,1) not null primary key,
    --наименование прививки
  name nchar(150) not null,
    --стоимость
  summ           int not null
)

-- связка хозяев и животных
create table person_animal_map (
  person_id   int not null foreign key references person(id),
  animal_id int not null foreign key references animal(id)
)

-- выполнение услуги доктора
create table get_service (
  person_id   int not null foreign key references person(id),
  animal_id int not null foreign key references animal(id),
  doctor_id int not null foreign key references doctor(id),
  service_id int not null foreign key references services_doctor(id),
    --дата и время выполнения услуги
  date_appointment    datetime     not null,
    --пришли ли на прием (да, нет, другое)
  check_get     nchar(20),
  diagnosis		nchar(150)
)

-- выполнение прививки доктором
create table get_vaccinations (
  person_id   int not null foreign key references person(id),
  animal_id int not null foreign key references animal(id),
  doctor_id int not null foreign key references doctor(id),
  vaccination_id int not null foreign key references vaccinations(id),
    --дата и время выполнения услуги
  date_appointment    datetime     not null,
    --пришли ли на прием (да, нет, другое)
  check_get     nchar(20)
)