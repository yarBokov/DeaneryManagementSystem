--
-- PostgreSQL database dump
--

-- Dumped from database version 15.4
-- Dumped by pg_dump version 15.4

-- Started on 2024-04-30 14:44:26

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 244 (class 1255 OID 16762)
-- Name: check_mark_range_func(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.check_mark_range_func() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
 	IF NEW.value NOT BETWEEN 2 AND 5 THEN
        RAISE SQLSTATE '45000' 
        USING  MESSAGE = 'Оценка должна быть в интервале [2..5]';
    END IF;
return new;
end;
$$;


ALTER FUNCTION public.check_mark_range_func() OWNER TO postgres;

--
-- TOC entry 246 (class 1255 OID 16772)
-- Name: check_subject_refs_count_func(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.check_subject_refs_count_func() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
declare
	ref_count integer;
begin
	SELECT COUNT(*) INTO ref_count FROM marks WHERE subject_id = OLD.id;
    IF ref_count > 0 THEN
        ROLLBACK;
    END IF;
return new;
end;
$$;


ALTER FUNCTION public.check_subject_refs_count_func() OWNER TO postgres;

--
-- TOC entry 245 (class 1255 OID 16770)
-- Name: check_subject_refs_func(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.check_subject_refs_func() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
declare
	ref_count integer;
begin
	select count(*) into ref_count from marks where subject_id = old.id;
	if ref_count > 0 and new.name <> old.name then
		raise sqlstate '45000'
		using message = 'Нельзя изменить наименование предмета, на который есть ссылки';
	end if;
return new;
end;
$$;


ALTER FUNCTION public.check_subject_refs_func() OWNER TO postgres;

--
-- TOC entry 247 (class 1255 OID 16911)
-- Name: get_avg_marks(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_avg_marks(start_date integer, end_date integer) RETURNS TABLE(year integer, average_mark numeric, diff numeric)
    LANGUAGE plpgsql
    AS $$
DECLARE 
	prev_avg numeric;
    curr_avg numeric;
    curr_year integer;
    prev_year integer;
	diff numeric;
    done boolean DEFAULT FALSE;
    cur CURSOR FOR 
		SELECT DISTINCT groups.year FROM marks
		join people on marks.student_id = people.id
		join groups on people.group_id = groups.id
		WHERE groups.year BETWEEN start_date AND end_date;
BEGIN
	drop table if exists results;
	create temporary table results(year integer, average_mark numeric, diff numeric);

    OPEN cur;

    FETCH cur INTO curr_year;
    prev_year := curr_year - 1;

    WHILE NOT done LOOP	
        curr_avg := (SELECT AVG(value) FROM marks 
		JOIN people ON marks.student_id = people.id
		JOIN groups ON people.group_id = groups.id
		WHERE groups.year = curr_year);
		
		prev_avg := (SELECT AVG(value) FROM marks 
		JOIN people ON marks.student_id = people.id
		JOIN groups ON people.group_id = groups.id
		WHERE groups.year = prev_year);
		
		diff := curr_avg - prev_avg;
        prev_year := curr_year;
		insert into results values(prev_year, curr_avg, diff);
        FETCH cur INTO curr_year;
		IF NOT FOUND THEN
			done := true;
		END IF;
    END LOOP;
	
    CLOSE cur;
	return query
		Select * from results;
END
$$;


ALTER FUNCTION public.get_avg_marks(start_date integer, end_date integer) OWNER TO postgres;

--
-- TOC entry 230 (class 1255 OID 16751)
-- Name: group_avg(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.group_avg(start_date integer, end_date integer) RETURNS TABLE(group_name character varying, avg_mark numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    return query
		SELECT g.name AS group_name, AVG(m.value) AS avg_mark
		FROM marks m
		JOIN people p ON m.student_id = p.id
		JOIN groups g ON p.group_id = g.id
		WHERE g.year BETWEEN start_date AND end_date
		GROUP BY g.name;
END 
$$;


ALTER FUNCTION public.group_avg(start_date integer, end_date integer) OWNER TO postgres;

--
-- TOC entry 232 (class 1255 OID 16755)
-- Name: group_min_avg_mark_by_teacher(integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.group_min_avg_mark_by_teacher(IN teacherid integer, OUT group_name character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    SELECT g.name INTO group_name
    FROM groups g
    JOIN people p ON g.id = p.group_id
    JOIN marks m ON p.id = m.student_id
    WHERE m.teacher_id = teacherID
    GROUP BY g.id
    ORDER BY AVG(m.value)
    LIMIT 1;
END
$$;


ALTER PROCEDURE public.group_min_avg_mark_by_teacher(IN teacherid integer, OUT group_name character varying) OWNER TO postgres;

--
-- TOC entry 231 (class 1255 OID 16752)
-- Name: subject_avg(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.subject_avg() RETURNS TABLE(subject character varying, avg_mark numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    return query
		SELECT s.name AS subject, AVG(m.value) AS avg_mark
		FROM marks m
		JOIN subjects s ON m.subject_id = s.id
		join people p on m.student_id = p.id
		join groups g on p.group_id = g.id
		WHERE g.year = 2023
		GROUP BY s.name;
END 
$$;


ALTER FUNCTION public.subject_avg() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 218 (class 1259 OID 16479)
-- Name: groups; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.groups (
    id integer NOT NULL,
    name character varying(50),
    year integer
);


ALTER TABLE public.groups OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16482)
-- Name: groups_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.groups ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.groups_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 226 (class 1259 OID 16991)
-- Name: keys; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.keys (
    id integer NOT NULL,
    "accessKey" character varying(256)
);


ALTER TABLE public.keys OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 17005)
-- Name: keys_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.keys ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.keys_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 220 (class 1259 OID 16483)
-- Name: marks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.marks (
    id integer NOT NULL,
    student_id integer,
    subject_id integer,
    teacher_id integer,
    value integer,
    term integer
);


ALTER TABLE public.marks OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16486)
-- Name: marks_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.marks ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.marks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 222 (class 1259 OID 16487)
-- Name: people; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.people (
    id integer NOT NULL,
    first_name character varying(20),
    second_name character varying(20),
    pather_name character varying(20),
    group_id integer,
    type "char"
);


ALTER TABLE public.people OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 16490)
-- Name: people_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.people ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.people_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 224 (class 1259 OID 16491)
-- Name: subjects; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.subjects (
    id integer NOT NULL,
    name character varying(50)
);


ALTER TABLE public.subjects OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 16494)
-- Name: subjects_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.subjects ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.subjects_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 228 (class 1259 OID 16995)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    "person_Id" integer,
    "hashedPassword" character varying(256),
    "key_Id" integer
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 16994)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.users ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 3372 (class 0 OID 16479)
-- Dependencies: 218
-- Data for Name: groups; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.groups (id, name, year) FROM stdin;
2000	TEACHERS IN HUMAN	\N
1	5130/1_2023	2023
2	5130/2_2023	2023
4	3690/1_2023	2023
5	3690/2_2023	2023
7	5130/2_2022	2022
9	3690/1_2022	2022
3	5130/3_2023	2023
1000	TEACHERS IN TECH	\N
14	6150/1_2023	2023
6	5130/1_2022	2022
9999	DEFAULT TEACHER	\N
8888	WORKERS-ADMINS	\N
\.


--
-- TOC entry 3380 (class 0 OID 16991)
-- Dependencies: 226
-- Data for Name: keys; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.keys (id, "accessKey") FROM stdin;
1	WZZB6PUaAhLq40XEznpEXA==;ZtL8aljbJqAuZ2iJB45df1jo5qmz7z2udyD4mkLekk4=
2	AnqI6BapUGfyHJPjOJm38w==;7ICCMR6DEt3/jyRKA2ffLrUuXRyc0oPWwKDf623ZA2U=
\.


--
-- TOC entry 3374 (class 0 OID 16483)
-- Dependencies: 220
-- Data for Name: marks; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.marks (id, student_id, subject_id, teacher_id, value, term) FROM stdin;
70	11	1	1	3	2
77	11	3	2	2	2
11	11	3	2	3	1
16	19	2	1	4	1
24	19	5	4	4	1
25	18	5	4	4	1
26	17	5	4	4	1
30	20	13	7	4	1
31	21	13	7	4	1
32	20	14	8	2	1
33	21	14	8	5	1
35	21	2	1	4	1
37	20	2	1	3	1
38	20	3	2	2	1
39	21	3	2	2	1
40	20	4	3	3	1
41	21	4	3	4	1
42	21	5	4	4	1
43	20	5	4	3	1
44	20	15	9	3	1
45	21	15	9	3	1
50	33	1	1	5	1
51	33	2	1	5	1
52	33	3	2	4	1
53	34	3	2	4	1
54	34	2	1	3	1
55	34	1	1	5	1
56	35	1	1	5	1
57	35	2	1	4	1
58	35	3	2	3	1
59	35	4	3	3	1
60	33	4	3	5	1
47	21	7	38	4	1
46	20	7	38	5	1
63	11	2	1	3	1
64	12	2	1	4	1
65	13	2	1	4	1
66	14	2	1	4	1
69	11	2	1	4	2
71	12	1	1	3	2
72	12	2	1	4	2
73	13	2	1	3	2
74	13	1	1	3	2
75	14	2	1	5	2
76	14	1	1	5	2
\.


--
-- TOC entry 3376 (class 0 OID 16487)
-- Dependencies: 222
-- Data for Name: people; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.people (id, first_name, second_name, pather_name, group_id, type) FROM stdin;
9	Дарья	Пономарёва	Филипповна	2000	P
20	Саида	Шестакова	Андреевна	3	S
21	Валерий	Калинин	Антонович	3	S
5	Сергей	Смирнов	Ефимович	1000	P
7	Вячеслав	Журавлёв	Яковлевич	2000	P
8	Вероника	Куприянова	Борисовна	2000	P
28	Александр	Русаков	Георгиевич	14	S
30	Святослав	Соболев	Александрович	14	S
31	Андрей	Гаврилов	Эдуардович	14	S
32	Анна	Панфилова	Андреевна	14	S
34	Анис	Садыков	Айдарович	6	S
35	Анна	Емелина	Романовна	6	S
13	Алексей	Гаврилов	Эдуардович	1	S
40	Иван	Иванов	Иванович	1	S
1	Андрей	Антонов	Андреевич	1000	P
41	втаиываио	цйуавцлад	ывлаворапро	1000	P
2	Борис	Наумов	Валентинович	1000	P
4	Андрей	Калинин	Юрьевич	1000	P
33	Карим	Сунгатов	Айдарович	6	S
17	Яков	Егоров	Эльдарович	2	S
18	Игнат	Кондратьев	Яковлевич	2	S
19	Юлия	Суханова	Даниловна	2	S
11	Ярослав	Боков	Александрович	1	S
12	Святослав	Соболев	Александрович	1	S
14	Анна	Панфилова	Андреевна	1	S
15	Арина	Малецкая	Игоревна	2	S
16	Артём	Петухов	Даниилович	2	S
3	Ольга	Анисимова	Петровна	1000	P
9999	DEFAULT	TEACHER	BUFFER	9999	P
38	Модели	Мат	Моделевич	1000	P
29	Михаил	Боков	Александрович	14	S
881	Алексей	Волков	Геннадиевич	8888	W
882	Никита	Ефремов	Анатольевич	8888	W
883	Галина	Тихонова	Кирилловна	8888	W
39	Алексей	Петров	Дмитриевич	2000	P
\.


--
-- TOC entry 3378 (class 0 OID 16491)
-- Dependencies: 224
-- Data for Name: subjects; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.subjects (id, name) FROM stdin;
2	Statistics
3	Programming
4	Databases
7	Mathematical models
8	\N
9	\N
10	\N
11	\N
12	\N
13	Literature
14	History
15	Philosophy
5	GNU/Linux
1	Math
6	Physic
\.


--
-- TOC entry 3382 (class 0 OID 16995)
-- Dependencies: 228
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, "person_Id", "hashedPassword", "key_Id") FROM stdin;
2	11	AQ+yLQxqYYgJl8YaCHcqgQ==;xIfnoLqoea7ifnQ+A9E+pgKa8bRUdv4kuYWpjYNzTbY=	\N
3	881	/9uoxzj6Y0vy7dzxDJGdiA==;aY/jO/ODlEcS1wNGHB1pHU4niFaqhlHEVroB3/jIMMs=	1
5	882	kSlYS2HRPdAH/7En3G63bQ==;+mDkpksS4TL1+CJ8D5znySehQh1S78goTAm0A58GdqQ=	1
6	12	3DJ+HLVijcLBvtu0TpzREQ==;AxzVdBq2W2q3Pl19VLCkOCE7eR9k0hPJQaJjR5DQvYA=	\N
7	33	l42jMtFOvMl3KXZowBtfMw==;enXWVQerq82kk5Att66Fbp+Tts/sfhRBuTLLD7Xktwg=	\N
\.


--
-- TOC entry 3389 (class 0 OID 0)
-- Dependencies: 219
-- Name: groups_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.groups_id_seq', 17, true);


--
-- TOC entry 3390 (class 0 OID 0)
-- Dependencies: 229
-- Name: keys_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.keys_id_seq', 2, true);


--
-- TOC entry 3391 (class 0 OID 0)
-- Dependencies: 221
-- Name: marks_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.marks_id_seq', 77, true);


--
-- TOC entry 3392 (class 0 OID 0)
-- Dependencies: 223
-- Name: people_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.people_id_seq', 41, true);


--
-- TOC entry 3393 (class 0 OID 0)
-- Dependencies: 225
-- Name: subjects_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.subjects_id_seq', 15, true);


--
-- TOC entry 3394 (class 0 OID 0)
-- Dependencies: 227
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 7, true);


--
-- TOC entry 3210 (class 2606 OID 16496)
-- Name: groups groups_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.groups
    ADD CONSTRAINT groups_pkey PRIMARY KEY (id);


--
-- TOC entry 3218 (class 2606 OID 17010)
-- Name: keys keys_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.keys
    ADD CONSTRAINT keys_pkey PRIMARY KEY (id);


--
-- TOC entry 3212 (class 2606 OID 16498)
-- Name: marks marks_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.marks
    ADD CONSTRAINT marks_pkey PRIMARY KEY (id);


--
-- TOC entry 3214 (class 2606 OID 16500)
-- Name: people people_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.people
    ADD CONSTRAINT people_pkey PRIMARY KEY (id);


--
-- TOC entry 3216 (class 2606 OID 16502)
-- Name: subjects subjects_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subjects
    ADD CONSTRAINT subjects_pkey PRIMARY KEY (id);


--
-- TOC entry 3220 (class 2606 OID 16999)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 3227 (class 2620 OID 16763)
-- Name: marks check_mark_range; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER check_mark_range BEFORE INSERT ON public.marks FOR EACH ROW EXECUTE FUNCTION public.check_mark_range_func();


--
-- TOC entry 3228 (class 2620 OID 16771)
-- Name: subjects check_subject_references; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER check_subject_references BEFORE UPDATE ON public.subjects FOR EACH ROW EXECUTE FUNCTION public.check_subject_refs_func();


--
-- TOC entry 3229 (class 2620 OID 16773)
-- Name: subjects check_subject_references_on_delete; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER check_subject_references_on_delete BEFORE DELETE ON public.subjects FOR EACH ROW EXECUTE FUNCTION public.check_subject_refs_count_func();


--
-- TOC entry 3221 (class 2606 OID 16503)
-- Name: marks fk_marks_people1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.marks
    ADD CONSTRAINT fk_marks_people1 FOREIGN KEY (student_id) REFERENCES public.people(id);


--
-- TOC entry 3222 (class 2606 OID 16508)
-- Name: marks fk_marks_people2; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.marks
    ADD CONSTRAINT fk_marks_people2 FOREIGN KEY (teacher_id) REFERENCES public.people(id);


--
-- TOC entry 3223 (class 2606 OID 16513)
-- Name: marks fk_marks_subjects; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.marks
    ADD CONSTRAINT fk_marks_subjects FOREIGN KEY (subject_id) REFERENCES public.subjects(id) NOT VALID;


--
-- TOC entry 3224 (class 2606 OID 16518)
-- Name: people fk_people_groups; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.people
    ADD CONSTRAINT fk_people_groups FOREIGN KEY (group_id) REFERENCES public.groups(id);


--
-- TOC entry 3225 (class 2606 OID 17011)
-- Name: users fk_users_keys; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_users_keys FOREIGN KEY ("key_Id") REFERENCES public.keys(id) NOT VALID;


--
-- TOC entry 3226 (class 2606 OID 17000)
-- Name: users fk_users_people; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_users_people FOREIGN KEY ("person_Id") REFERENCES public.people(id) NOT VALID;


-- Completed on 2024-04-30 14:44:26

--
-- PostgreSQL database dump complete
--

